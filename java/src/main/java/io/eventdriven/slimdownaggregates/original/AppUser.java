package io.eventdriven.slimdownaggregates.original;

import io.eventdriven.slimdownaggregates.original.core.*;
import io.eventdriven.slimdownaggregates.original.entities.*;
import jakarta.persistence.*;
import org.apache.commons.lang3.StringUtils;
import org.springframework.security.core.GrantedAuthority;
import org.springframework.security.core.authority.SimpleGrantedAuthority;
import org.springframework.security.core.userdetails.User;

import java.time.LocalDate;
import java.util.*;

@Entity
@Table(name = "m_appuser", uniqueConstraints = @UniqueConstraint(columnNames = { "username" }, name = "username_org"))
public class AppUser extends AbstractPersistableCustom implements PlatformUser {

  @Column(name = "email", nullable = false, length = 100)
  private String email;

  @Column(name = "username", nullable = false, length = 100)
  private String username;

  @Column(name = "firstname", nullable = false, length = 100)
  private String firstname;

  @Column(name = "lastname", nullable = false, length = 100)
  private String lastname;

  @Column(name = "password", nullable = false)
  private String password;

  @Column(name = "nonexpired", nullable = false)
  private boolean accountNonExpired;

  @Column(name = "nonlocked", nullable = false)
  private boolean accountNonLocked;

  @Column(name = "nonexpired_credentials", nullable = false)
  private boolean credentialsNonExpired;

  @Column(name = "enabled", nullable = false)
  private boolean enabled;

  @Column(name = "firsttime_login_remaining", nullable = false)
  private boolean firstTimeLoginRemaining;

  @Column(name = "is_deleted", nullable = false)
  private boolean deleted;

  @ManyToOne
  @JoinColumn(name = "office_id", nullable = false)
  private Office office;

  @ManyToOne
  @JoinColumn(name = "staff_id", nullable = true)
  private Staff staff;

  @ManyToMany(fetch = FetchType.EAGER)
  @JoinTable(name = "m_appuser_role", joinColumns = @JoinColumn(name = "appuser_id"), inverseJoinColumns = @JoinColumn(name = "role_id"))
  private Set<Role> roles;

  @Column(name = "last_time_password_updated")
  private LocalDate lastTimePasswordUpdated;

  @Column(name = "password_never_expires", nullable = false)
  private boolean passwordNeverExpires;

  @Column(name = "is_self_service_user", nullable = false)
  private boolean isSelfServiceUser;

  @OneToMany(cascade = CascadeType.ALL, orphanRemoval = true, fetch = FetchType.EAGER, mappedBy = "appUser")
  private Set<AppUserClientMapping> appUserClientMappings = new HashSet<>();

  @Column(name = "cannot_change_password", nullable = true)
  private Boolean cannotChangePassword;

  public static AppUser fromJson(final Office userOffice, final Staff linkedStaff, final Set<Role> allRoles,
                                 final Collection<Client> clients, final JsonCommand command) {

    final String username = command.stringValueOfParameterNamed("username");
    String password = command.stringValueOfParameterNamed("password");
    final Boolean sendPasswordToEmail = command.booleanObjectValueOfParameterNamed("sendPasswordToEmail");

    if (sendPasswordToEmail) {
      password = new RandomPasswordGenerator(13).generate();
    }

    boolean passwordNeverExpire = false;

    if (command.parameterExists(AppUserConstants.PASSWORD_NEVER_EXPIRES)) {
      passwordNeverExpire = command.booleanPrimitiveValueOfParameterNamed(AppUserConstants.PASSWORD_NEVER_EXPIRES);
    }

    final boolean userEnabled = true;
    final boolean userAccountNonExpired = true;
    final boolean userCredentialsNonExpired = true;
    final boolean userAccountNonLocked = true;
    final boolean cannotChangePassword = false;

    final Collection<SimpleGrantedAuthority> authorities = new ArrayList<>();
    authorities.add(new SimpleGrantedAuthority("DUMMY_ROLE_NOT_USED_OR_PERSISTED_TO_AVOID_EXCEPTION"));

    final User user = new User(username, password, userEnabled, userAccountNonExpired, userCredentialsNonExpired, userAccountNonLocked,
      authorities);

    final String email = command.stringValueOfParameterNamed("email");
    final String firstname = command.stringValueOfParameterNamed("firstname");
    final String lastname = command.stringValueOfParameterNamed("lastname");

    final boolean isSelfServiceUser = command.booleanPrimitiveValueOfParameterNamed(AppUserConstants.IS_SELF_SERVICE_USER);

    return new AppUser(userOffice, user, allRoles, email, firstname, lastname, linkedStaff, passwordNeverExpire, isSelfServiceUser,
      clients, cannotChangePassword);
  }

  protected AppUser() {
    this.accountNonLocked = false;
    this.credentialsNonExpired = false;
    this.roles = new HashSet<>();
  }

  public AppUser(final Office office, final User user, final Set<Role> roles, final String email, final String firstname,
                 final String lastname, final Staff staff, final boolean passwordNeverExpire, final boolean isSelfServiceUser,
                 final Collection<Client> clients, final Boolean cannotChangePassword) {
    this.office = office;
    this.email = email.trim();
    this.username = user.getUsername().trim();
    this.firstname = firstname.trim();
    this.lastname = lastname.trim();
    this.password = user.getPassword().trim();
    this.accountNonExpired = user.isAccountNonExpired();
    this.accountNonLocked = user.isAccountNonLocked();
    this.credentialsNonExpired = user.isCredentialsNonExpired();
    this.enabled = user.isEnabled();
    this.roles = roles;
    this.firstTimeLoginRemaining = true;
    this.lastTimePasswordUpdated = DateUtils.getLocalDateOfTenant();
    this.staff = staff;
    this.passwordNeverExpires = passwordNeverExpire;
    this.isSelfServiceUser = isSelfServiceUser;
    this.appUserClientMappings = createAppUserClientMappings(clients);
    this.cannotChangePassword = cannotChangePassword;
  }

  public EnumOptionData organisationalRoleData() {
    EnumOptionData organisationalRole = null;
    if (this.staff != null) {
      organisationalRole = this.staff.organisationalRoleData();
    }
    return organisationalRole;
  }

  public void updatePassword(final String encodePassword) {
    if (cannotChangePassword != null && cannotChangePassword == true) {
      throw new NoAuthorizationException("Password of this user may not be modified");
    }

    this.password = encodePassword;
    this.firstTimeLoginRemaining = false;
    this.lastTimePasswordUpdated = DateUtils.getBusinessLocalDate();

  }

  public void changeOffice(final Office newValue) {
    this.office = newValue;
  }

  public void changeStaff(final Staff newValue) {
    this.staff = newValue;
  }

  public void updateRoles(final Set<Role> allRoles) {
    if (!allRoles.isEmpty()) {
      this.roles.clear();
      this.roles = allRoles;
    }
  }

  public void setUsername(final String newValue) {
    // TODO Remove this check once we are identifying system user based on user ID
    if (isSystemUser()) {
      throw new NoAuthorizationException("User name of current system user may not be modified");
    }

    this.username = newValue;
  }

  public void setFirstname(final String newValue) {
    this.firstname = newValue;
  }

  public void setLastname(final String newValue) {
    this.lastname = newValue;
  }

  public void setEmail(final String newValue) {
    this.email = newValue;
  }

  public void setPasswordNeverExpires(final boolean newValue) {
    this.passwordNeverExpires = newValue;
  }

  public void setIsSelfServiceUser(final boolean newValue) {
    this.isSelfServiceUser = newValue;
  }

  public void setClients(final Collection<Client> clients) {
    if (this.isSelfServiceUser) {
      Set<AppUserClientMapping> newClients = createAppUserClientMappings(clients);
      if (this.appUserClientMappings == null) {
        this.appUserClientMappings = new HashSet<>();
      } else {
        this.appUserClientMappings.retainAll(newClients);
      }
      this.appUserClientMappings.addAll(newClients);
    } else {
      if (this.appUserClientMappings != null) {
        this.appUserClientMappings.clear();
      }
    }
  }
  private String[] getRolesAsIdStringArray() {
    final List<String> roleIds = new ArrayList<>();

    for (final Role role : this.roles) {
      roleIds.add(role.getId().toString());
    }

    return roleIds.toArray(new String[roleIds.size()]);
  }

  /**
   * Delete is a <i>soft delete</i>. Updates flag so it wont appear in query/report results.
   *
   * Any fields with unique constraints and prepended with id of record.
   */
  public void delete() {
    if (isSystemUser()) {
      throw new NoAuthorizationException("User configured as the system user cannot be deleted");
    }

    this.deleted = true;
    this.enabled = false;
    this.accountNonExpired = false;
    this.firstTimeLoginRemaining = true;
    this.username = getId() + "_DELETED_" + this.username;
    this.roles.clear();
  }

  public boolean isDeleted() {
    return this.deleted;
  }

  public boolean isSystemUser() {
    // TODO Determine system user by ID not by user name
    if (this.username.equals(AppUserConstants.SYSTEM_USER_NAME)) {
      return true;
    }

    return false;
  }

  @Override
  public Collection<GrantedAuthority> getAuthorities() {
    return populateGrantedAuthorities();
  }

  private List<GrantedAuthority> populateGrantedAuthorities() {
    final List<GrantedAuthority> grantedAuthorities = new ArrayList<>();
    for (final Role role : this.roles) {
      final Collection<Permission> permissions = role.getPermissions();
      for (final Permission permission : permissions) {
        grantedAuthorities.add(new SimpleGrantedAuthority(permission.getCode()));
      }
    }
    return grantedAuthorities;
  }

  @Override
  public String getPassword() {
    return this.password;
  }

  @Override
  public String getUsername() {
    return this.username;
  }

  public String getDisplayName() {
    if (this.staff != null && StringUtils.isNotBlank(this.staff.displayName())) {
      return this.staff.displayName();
    }
    String firstName = StringUtils.isNotBlank(this.firstname) ? this.firstname : "";
    if (StringUtils.isNotBlank(this.lastname)) {
      return firstName + " " + this.lastname;
    }
    return firstName;
  }

  @Override
  public boolean isAccountNonExpired() {
    return this.accountNonExpired;
  }

  @Override
  public boolean isAccountNonLocked() {
    return this.accountNonLocked;
  }

  @Override
  public boolean isCredentialsNonExpired() {
    return this.credentialsNonExpired;
  }

  @Override
  public boolean isEnabled() {
    return this.enabled;
  }

  public boolean isBypassUser() {
    return hasAnyPermission("BYPASS_LOAN_WRITE_PROTECTION");
  }

  public String getFirstname() {
    return this.firstname;
  }

  public String getLastname() {
    return this.lastname;
  }

  public String getEmail() {
    return this.email;
  }

  public Set<Role> getRoles() {
    return this.roles;
  }

  public Office getOffice() {
    return this.office;
  }

  public Staff getStaff() {
    return this.staff;
  }

  public boolean getPasswordNeverExpires() {
    return this.passwordNeverExpires;
  }

  public LocalDate getLastTimePasswordUpdated() {
    return this.lastTimePasswordUpdated;
  }

  public boolean canNotApproveLoanInPast() {
    return hasNotPermissionForAnyOf("ALL_FUNCTIONS", "APPROVEINPAST_LOAN");
  }

  public boolean canNotRejectLoanInPast() {
    return hasNotPermissionForAnyOf("ALL_FUNCTIONS", "REJECTINPAST_LOAN");
  }

  public boolean canNotWithdrawByClientLoanInPast() {
    return hasNotPermissionForAnyOf("ALL_FUNCTIONS", "WITHDRAWINPAST_LOAN");
  }

  public boolean canNotDisburseLoanInPast() {
    return hasNotPermissionForAnyOf("ALL_FUNCTIONS", "DISBURSEINPAST_LOAN");
  }

  public boolean canNotMakeRepaymentOnLoanInPast() {
    return hasNotPermissionForAnyOf("ALL_FUNCTIONS", "REPAYMENTINPAST_LOAN");
  }

  public boolean hasNotPermissionForReport(final String reportName) {

    if (hasNotPermissionForAnyOf("ALL_FUNCTIONS", "ALL_FUNCTIONS_READ", "REPORTING_SUPER_USER", "READ_" + reportName)) {
      return true;
    }

    return false;
  }

  public boolean hasNotPermissionForDatatable(final String datatable, final String accessType) {

    final String matchPermission = accessType + "_" + datatable;

    if (accessType.equalsIgnoreCase("READ")) {

      if (hasNotPermissionForAnyOf("ALL_FUNCTIONS", "ALL_FUNCTIONS_READ", matchPermission)) {
        return true;
      }

      return false;
    }

    if (hasNotPermissionForAnyOf("ALL_FUNCTIONS", matchPermission)) {
      return true;
    }

    return false;
  }

  public boolean hasNotPermissionForAnyOf(final String... permissionCodes) {
    boolean hasNotPermission = true;
    for (final String permissionCode : permissionCodes) {
      final boolean checkPermission = hasPermissionTo(permissionCode);
      if (checkPermission) {
        hasNotPermission = false;
        break;
      }
    }
    return hasNotPermission;
  }

  /**
   * Checks whether the user has a given permission explicitly.
   *
   * @param permissionCode
   *            the permission code to check for.
   * @return whether the user has the specified permission
   */
  public boolean hasSpecificPermissionTo(final String permissionCode) {
    boolean hasPermission = false;
    for (final Role role : this.roles) {
      if (role.hasPermissionTo(permissionCode)) {
        hasPermission = true;
        break;
      }
    }
    return hasPermission;
  }

  public void validateHasReadPermission(final String resourceType) {
    validateHasPermission("READ", resourceType);
  }

  public void validateHasCreatePermission(final String resourceType) {
    validateHasPermission("CREATE", resourceType);
  }

  public void validateHasUpdatePermission(final String resourceType) {
    validateHasPermission("UPDATE", resourceType);
  }

  public void validateHasDeletePermission(final String resourceType) {
    validateHasPermission("DELETE", resourceType);
  }

  private void validateHasPermission(final String prefix, final String resourceType) {
    final String authorizationMessage = "User has no authority to " + prefix + " " + resourceType.toLowerCase() + "s";
    final String matchPermission = prefix + "_" + resourceType.toUpperCase();

    if (!hasNotPermissionForAnyOf("ALL_FUNCTIONS", "ALL_FUNCTIONS_READ", matchPermission)) {
      return;
    }

    throw new NoAuthorizationException(authorizationMessage);
  }

  private boolean hasNotPermissionTo(final String permissionCode) {
    return !hasPermissionTo(permissionCode);
  }

  private boolean hasPermissionTo(final String permissionCode) {
    boolean hasPermission = hasAllFunctionsPermission();
    if (!hasPermission) {
      for (final Role role : this.roles) {
        if (role.hasPermissionTo(permissionCode)) {
          hasPermission = true;
          break;
        }
      }
    }
    return hasPermission;
  }

  private boolean hasAllFunctionsPermission() {
    boolean match = false;
    for (final Role role : this.roles) {
      if (role.hasPermissionTo("ALL_FUNCTIONS")) {
        match = true;
        break;
      }
    }
    return match;
  }

  public boolean hasIdOf(final Long userId) {
    return getId().equals(userId);
  }

  private boolean hasNotAnyPermission(final List<String> permissions) {
    return !hasAnyPermission(permissions);
  }

  public boolean hasAnyPermission(String... permissions) {
    return hasAnyPermission(Arrays.asList(permissions));
  }

  public boolean hasAnyPermission(final List<String> permissions) {
    boolean hasAtLeastOneOf = false;

    for (final String permissionCode : permissions) {
      if (hasPermissionTo(permissionCode)) {
        hasAtLeastOneOf = true;
        break;
      }
    }

    return hasAtLeastOneOf;
  }

  public void validateHasPermissionTo(final String function, final List<String> allowedPermissions) {
    if (hasNotAnyPermission(allowedPermissions)) {
      final String authorizationMessage = "User has no authority to: " + function;
      throw new NoAuthorizationException(authorizationMessage);
    }
  }

  public void validateHasPermissionTo(final String function) {
    if (hasNotPermissionTo(function)) {
      final String authorizationMessage = "User has no authority to: " + function;
      throw new NoAuthorizationException(authorizationMessage);
    }
  }

  public void validateHasReadPermission(final String function, final Long userId) {
    if (!("USER".equalsIgnoreCase(function) && userId.equals(getId()))) {
      validateHasReadPermission(function);
    }
  }

  public void validateHasCheckerPermissionTo(final String function) {
    final String checkerPermissionName = function.toUpperCase() + "_CHECKER";
    if (hasNotPermissionTo("CHECKER_SUPER_USER") && hasNotPermissionTo(checkerPermissionName)) {
      final String authorizationMessage = "User has no authority to be a checker for: " + function;
      throw new NoAuthorizationException(authorizationMessage);
    }
  }

  public void validateHasDatatableReadPermission(final String datatable) {
    if (hasNotPermissionForDatatable(datatable, "READ")) {
      throw new NoAuthorizationException("Not authorised to read datatable: " + datatable);
    }
  }

  public Long getStaffId() {
    Long staffId = null;
    if (this.staff != null) {
      staffId = this.staff.getId();
    }
    return staffId;
  }

  public String getStaffDisplayName() {
    String staffDisplayName = null;
    if (this.staff != null) {
      staffDisplayName = this.staff.displayName();
    }
    return staffDisplayName;
  }

  public String getEncodedPassword(final JsonCommand command, final PlatformPasswordEncoder platformPasswordEncoder) {
    final String passwordParamName = "password";
    final String passwordEncodedParamName = "passwordEncoded";
    String passwordEncodedValue = null;

    if (command.hasParameter(passwordParamName)) {
      if (command.isChangeInPasswordParameterNamed(passwordParamName, this.password, platformPasswordEncoder, getId())) {

        passwordEncodedValue = command.passwordValueOfParameterNamed(passwordParamName, platformPasswordEncoder, getId());

      }
    } else if (command.hasParameter(passwordEncodedParamName)) {
      if (command.isChangeInStringParameterNamed(passwordEncodedParamName, this.password)) {

        passwordEncodedValue = command.stringValueOfParameterNamed(passwordEncodedParamName);

      }
    }

    return passwordEncodedValue;
  }

  public boolean isNotEnabled() {
    return !isEnabled();
  }

  public boolean isSelfServiceUser() {
    return this.isSelfServiceUser;
  }

  public Set<AppUserClientMapping> getAppUserClientMappings() {
    return this.appUserClientMappings;
  }

  private Set<AppUserClientMapping> createAppUserClientMappings(Collection<Client> clients) {
    Set<AppUserClientMapping> newAppUserClientMappings = null;
    if (clients != null && clients.size() > 0) {
      newAppUserClientMappings = new HashSet<>();
      for (Client client : clients) {
        newAppUserClientMappings.add(new AppUserClientMapping(this, client));
      }
    }
    return newAppUserClientMappings;
  }

  @Override
  public String toString() {
    return "AppUser [username=" + this.username + ", getId()=" + this.getId() + "]";
  }
}


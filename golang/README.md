### INSTALL

```bash
go install github.com/cespare/reflex@latest
go get -u github.com/kyoh86/richgo
```

### RUN 

```bash
reflex -r '\.go$' -d none -s -- sh -c \
'clear && richgo run ./main.go' 
```

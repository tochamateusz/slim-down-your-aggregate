import { DEFAULT_POSITIVE_NUMBER, PositiveNumber } from '#core/typing';
import { DomainEvent } from '../../../infrastructure/events';
import { Published } from '../published';

export class InPrint {
  constructor(private totalCopies: PositiveNumber) {}

  static moveToPublished(state: InPrint): Published {
    return {
      type: 'Published',
      data: { totalCopies: state.totalCopies },
    };
  }

  public static evolve(_: InPrint, event: InPrintEvent): InPrint {
    const { type, data } = event;

    switch (type) {
      case 'MovedToPrinting': {
        return new InPrint(data.totalCopies);
      }
    }
  }

  public static readonly initial = new InPrint(DEFAULT_POSITIVE_NUMBER);
}

export type MovedToPrinting = DomainEvent<
  'MovedToPrinting',
  { totalCopies: PositiveNumber }
>;

export type InPrintEvent = MovedToPrinting;

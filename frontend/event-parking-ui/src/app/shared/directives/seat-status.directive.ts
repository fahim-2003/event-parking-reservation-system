import {
  Directive,
  ElementRef,
  Input,
  OnChanges,
  Renderer2,
  inject
} from '@angular/core';

@Directive({
  selector: '[appSeatStatus]',
  standalone: true
})
export class SeatStatusDirective implements OnChanges {
  @Input('appSeatStatus') status = '';

  private readonly element =
    inject<ElementRef<HTMLElement>>(ElementRef);

  private readonly renderer =
    inject(Renderer2);

  ngOnChanges(): void {
    const normalized =
      this.status.trim().toLowerCase();

    const classNames = [
      'seat-status-available',
      'seat-status-held',
      'seat-status-booked'
    ];

    for (const className of classNames) {
      this.renderer.removeClass(
        this.element.nativeElement,
        className
      );
    }

    if (normalized === 'available') {
      this.renderer.addClass(
        this.element.nativeElement,
        'seat-status-available'
      );
      return;
    }

    if (normalized === 'held') {
      this.renderer.addClass(
        this.element.nativeElement,
        'seat-status-held'
      );
      return;
    }

    if (normalized === 'booked') {
      this.renderer.addClass(
        this.element.nativeElement,
        'seat-status-booked'
      );
    }
  }
}

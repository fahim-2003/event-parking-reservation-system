import {
  Directive,
  ElementRef,
  HostListener,
  Renderer2,
  inject
} from '@angular/core';

@Directive({
  selector: '[appHighlight]',
  standalone: true
})
export class HighlightDirective {
  private readonly element =
    inject<ElementRef<HTMLElement>>(ElementRef);

  private readonly renderer =
    inject(Renderer2);

  @HostListener('mouseenter')
  onMouseEnter(): void {
    this.renderer.setStyle(
      this.element.nativeElement,
      'transform',
      'translateY(-4px)'
    );

    this.renderer.setStyle(
      this.element.nativeElement,
      'box-shadow',
      '0 24px 60px rgba(15, 23, 42, 0.14)'
    );
  }

  @HostListener('mouseleave')
  onMouseLeave(): void {
    this.renderer.removeStyle(
      this.element.nativeElement,
      'transform'
    );

    this.renderer.removeStyle(
      this.element.nativeElement,
      'box-shadow'
    );
  }
}

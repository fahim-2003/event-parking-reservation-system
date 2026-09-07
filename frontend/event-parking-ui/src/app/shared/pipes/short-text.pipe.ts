import {
  Pipe,
  PipeTransform
} from '@angular/core';

@Pipe({
  name: 'shortText',
  standalone: true
})
export class ShortTextPipe implements PipeTransform {
  transform(
    value: string | null | undefined,
    maxLength = 80
  ): string {
    const text = value?.trim() ?? '';

    if (text.length <= maxLength) {
      return text;
    }

    return `${text.slice(0, maxLength).trimEnd()}...`;
  }
}

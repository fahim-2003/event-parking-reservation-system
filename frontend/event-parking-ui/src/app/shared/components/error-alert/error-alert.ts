import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-error-alert',
  imports: [],
  templateUrl: './error-alert.html',
  styleUrl: './error-alert.css'
})
export class ErrorAlert {
  @Input() title = 'Something went wrong';
  @Input() message = 'Please try again.';
  @Input() traceId = '';
}
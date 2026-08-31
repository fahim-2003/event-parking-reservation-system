import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-loading-indicator',
  imports: [],
  templateUrl: './loading-indicator.html',
  styleUrl: './loading-indicator.css'
})
export class LoadingIndicator {
  @Input() message = 'Loading...';
}
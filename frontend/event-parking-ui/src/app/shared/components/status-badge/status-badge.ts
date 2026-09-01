import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-status-badge',
  imports: [],
  templateUrl: './status-badge.html',
  styleUrl: './status-badge.css'
})
export class StatusBadge {
  @Input() status = '';

  get badgeClasses(): string {
    switch (this.status.toLowerCase()) {
      case 'available':
      case 'active':
      case 'confirmed':
        return 'border-emerald-200 bg-emerald-50 text-emerald-700';

      case 'selected':
        return 'border-indigo-200 bg-indigo-50 text-indigo-700';

      case 'held':
      case 'pending':
        return 'border-amber-200 bg-amber-50 text-amber-700';

      case 'booked':
      case 'occupied':
      case 'cancelled':
      case 'deactivated':
        return 'border-rose-200 bg-rose-50 text-rose-700';

      case 'expired':
      case 'unavailable':
        return 'border-slate-200 bg-slate-100 text-slate-600';

      default:
        return 'border-slate-200 bg-white/70 text-slate-700';
    }
  }
}
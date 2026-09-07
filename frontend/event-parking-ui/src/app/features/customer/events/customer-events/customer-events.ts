import { CommonModule } from '@angular/common';
import {
  Component,
  OnInit,
  inject
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import {
  ActivatedRoute,
  Router
} from '@angular/router';

import {
  EventFilters,
  EventItem,
  EventService
} from '../../../../core/services/event.service';
import { HighlightDirective } from '../../../../shared/directives/highlight.directive';

interface FilterOption {
  id: number;
  name: string;
}

@Component({
  selector: 'app-customer-events',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    HighlightDirective
  ],
  templateUrl: './customer-events.html',
  styleUrl: './customer-events.css'
})
export class CustomerEventsComponent implements OnInit {
  private readonly eventService = inject(EventService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  events: EventItem[] = [];
  venues: FilterOption[] = [];
  categories: FilterOption[] = [];

  search = '';
  dateFrom = '';
  dateTo = '';
  venueId: number | null = null;
  categoryId: number | null = null;

  loading = true;
  errorMessage = '';

  ngOnInit(): void {
    this.readFiltersFromUrl();
    this.loadFilterOptions();
    this.loadEvents();
  }

  applyFilters(): void {
    if (
      this.dateFrom &&
      this.dateTo &&
      this.dateFrom > this.dateTo
    ) {
      this.errorMessage =
        'From date cannot be later than To date.';
      return;
    }

    this.updateUrl();
  }

  clearFilters(): void {
    this.search = '';
    this.dateFrom = '';
    this.dateTo = '';
    this.venueId = null;
    this.categoryId = null;
    this.errorMessage = '';

    this.router.navigate(
      [],
      {
        relativeTo: this.route,
        queryParams: {}
      }
    ).then(() => {
      this.loadEvents();
    });
  }

  bookEvent(eventId: number): void {
    this.router.navigate([
      '/customer/bookings/create',
      eventId
    ]);
  }

  private readFiltersFromUrl(): void {
    const params =
      this.route.snapshot.queryParamMap;

    this.search =
      params.get('search') ?? '';

    this.dateFrom =
      params.get('dateFrom') ?? '';

    this.dateTo =
      params.get('dateTo') ?? '';

    this.venueId =
      this.parseOptionalNumber(
        params.get('venueId')
      );

    this.categoryId =
      this.parseOptionalNumber(
        params.get('categoryId')
      );
  }

  private updateUrl(): void {
    const queryParams = {
      search:
        this.search.trim() || null,

      dateFrom:
        this.dateFrom || null,

      dateTo:
        this.dateTo || null,

      venueId:
        this.venueId ?? null,

      categoryId:
        this.categoryId ?? null
    };

    this.router.navigate(
      [],
      {
        relativeTo: this.route,
        queryParams
      }
    ).then(() => {
      this.loadEvents();
    });
  }

  private loadEvents(): void {
    this.loading = true;
    this.errorMessage = '';

    this.eventService
      .getEvents(
        this.buildBackendFilters()
      )
      .subscribe({
        next: response => {
          this.events = response ?? [];
          this.loading = false;
        },

        error: () => {
          this.events = [];
          this.errorMessage =
            'Unable to load events. Please try again.';
          this.loading = false;
        }
      });
  }

  private loadFilterOptions(): void {
    this.eventService
      .getEvents()
      .subscribe({
        next: response => {
          this.venues =
            this.toUniqueOptions(
              response.map(event => ({
                id: event.venueId,
                name: event.venueName
              }))
            );

          this.categories =
            this.toUniqueOptions(
              response.map(event => ({
                id: event.eventCategoryId,
                name: event.categoryName
              }))
            );
        },

        error: () => {
          this.venues = [];
          this.categories = [];
        }
      });
  }

  private buildBackendFilters(): EventFilters {
    const filters: EventFilters = {};

    const trimmedSearch =
      this.search.trim();

    if (trimmedSearch) {
      filters.search = trimmedSearch;
    }

    if (this.dateFrom) {
      filters.dateFromUtc =
        `${this.dateFrom}T00:00:00.000Z`;
    }

    if (this.dateTo) {
      filters.dateToUtc =
        `${this.dateTo}T23:59:59.999Z`;
    }

    if (this.venueId !== null) {
      filters.venueId = this.venueId;
    }

    if (this.categoryId !== null) {
      filters.categoryId = this.categoryId;
    }

    return filters;
  }

  private parseOptionalNumber(
    value: string | null
  ): number | null {
    if (!value) {
      return null;
    }

    const parsed = Number(value);

    return Number.isFinite(parsed)
      ? parsed
      : null;
  }

  private toUniqueOptions(
    values: FilterOption[]
  ): FilterOption[] {
    const unique =
      new Map<number, string>();

    for (const value of values) {
      if (!unique.has(value.id)) {
        unique.set(
          value.id,
          value.name
        );
      }
    }

    return Array.from(
      unique.entries()
    )
      .map(([id, name]) => ({
        id,
        name
      }))
      .sort(
        (left, right) =>
          left.name.localeCompare(
            right.name
          )
      );
  }
}

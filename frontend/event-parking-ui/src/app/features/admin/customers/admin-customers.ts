import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import {
  Component,
  inject,
  OnInit,
  signal
} from '@angular/core';
import {
  FormBuilder,
  ReactiveFormsModule
} from '@angular/forms';
import { AdminCustomerSummary } from '../../../core/models/customer.models';
import { CustomerService } from '../../../core/services/customer.service';

@Component({
  selector: 'app-admin-customers',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    DatePipe
  ],
  templateUrl: './admin-customers.html',
  styleUrl: './admin-customers.css'
})
export class AdminCustomers implements OnInit {
  private readonly formBuilder = inject(FormBuilder);
  private readonly customerService = inject(CustomerService);

  readonly customers = signal<AdminCustomerSummary[]>([]);
  readonly selectedCustomer =
    signal<AdminCustomerSummary | null>(null);

  readonly isLoading = signal(true);
  readonly errorMessage = signal('');

  readonly searchForm = this.formBuilder.nonNullable.group({
    search: ['']
  });

  ngOnInit(): void {
    this.loadCustomers();
  }

  loadCustomers(): void {
    this.isLoading.set(true);
    this.errorMessage.set('');

    const search =
      this.searchForm.controls.search.value;

    this.customerService
      .searchCustomers(search)
      .subscribe({
        next: customers => {
          this.customers.set(customers);
          this.isLoading.set(false);
        },
        error: (error: HttpErrorResponse) => {
          this.isLoading.set(false);
          this.errorMessage.set(
            this.resolveErrorMessage(
              error,
              'Unable to load customers.'
            )
          );
        }
      });
  }

  clearSearch(): void {
    this.searchForm.reset({
      search: ''
    });

    this.selectedCustomer.set(null);
    this.loadCustomers();
  }

  viewCustomer(customerId: string): void {
    this.errorMessage.set('');

    this.customerService
      .getCustomerForAdmin(customerId)
      .subscribe({
        next: customer => {
          this.selectedCustomer.set(customer);
        },
        error: (error: HttpErrorResponse) => {
          this.errorMessage.set(
            this.resolveErrorMessage(
              error,
              'Unable to load customer details.'
            )
          );
        }
      });
  }

  closeDetails(): void {
    this.selectedCustomer.set(null);
  }

  private resolveErrorMessage(
    error: HttpErrorResponse,
    fallback: string
  ): string {
    return typeof error.error?.detail === 'string'
      ? error.error.detail
      : fallback;
  }
}

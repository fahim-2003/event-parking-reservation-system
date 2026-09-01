import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import {
  FormBuilder,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { finalize } from 'rxjs';
import { ConfirmationDialog } from '../../../../../shared/components/confirmation-dialog/confirmation-dialog';
import {
  CreateCategoryRequest,
  EventCategory
} from '../../models/category.model';
import { CategoryApiService } from '../../services/category-api.service';

@Component({
  selector: 'app-category-management',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    ConfirmationDialog
  ],
  templateUrl: './category-management.html',
  styleUrl: './category-management.css'
})
export class CategoryManagement {
  private readonly categoryApi = inject(CategoryApiService);
  private readonly formBuilder = inject(FormBuilder);

  readonly categories = signal<EventCategory[]>([]);
  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly errorMessage = signal('');
  readonly editingCategoryId = signal<number | null>(null);
  readonly pendingDeleteCategory = signal<EventCategory | null>(null);

  readonly categoryForm = this.formBuilder.nonNullable.group({
    name: ['', Validators.required]
  });

  constructor() {
    this.loadCategories();
  }

  loadCategories(): void {
    this.loading.set(true);
    this.errorMessage.set('');

    this.categoryApi
      .getAll()
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: categories => this.categories.set(categories),
        error: () =>
          this.errorMessage.set(
            'Unable to load categories. Please try again.'
          )
      });
  }

  submit(): void {
    if (this.categoryForm.invalid) {
      this.categoryForm.markAllAsTouched();
      return;
    }

    const formValue = this.categoryForm.getRawValue();

    const request: CreateCategoryRequest = {
      name: formValue.name.trim()
    };

    if (!request.name) {
      this.categoryForm.controls.name.setErrors({ required: true });
      return;
    }

    this.saving.set(true);
    this.errorMessage.set('');

    const editingId = this.editingCategoryId();

    const request$ =
      editingId === null
        ? this.categoryApi.create(request)
        : this.categoryApi.update(editingId, request);

    request$
      .pipe(finalize(() => this.saving.set(false)))
      .subscribe({
        next: () => {
          this.resetForm();
          this.loadCategories();
        },
        error: () =>
          this.errorMessage.set(
            'Unable to save category. The category name may already exist.'
          )
      });
  }

  editCategory(category: EventCategory): void {
    this.editingCategoryId.set(category.id);

    this.categoryForm.setValue({
      name: category.name
    });
  }

  cancelEdit(): void {
    this.resetForm();
  }

  deleteCategory(category: EventCategory): void {
    this.pendingDeleteCategory.set(category);
  }

  cancelDeleteCategory(): void {
    this.pendingDeleteCategory.set(null);
  }

  confirmDeleteCategory(): void {
    const category = this.pendingDeleteCategory();

    if (category === null) {
      return;
    }

    this.errorMessage.set('');

    this.categoryApi.delete(category.id).subscribe({
      next: () => {
        this.pendingDeleteCategory.set(null);
        this.loadCategories();
      },
      error: () => {
        this.pendingDeleteCategory.set(null);
        this.errorMessage.set(
          'Unable to delete category. It may be referenced by existing data.'
        );
      }
    });
  }

  private resetForm(): void {
    this.editingCategoryId.set(null);

    this.categoryForm.reset({
      name: ''
    });
  }
}

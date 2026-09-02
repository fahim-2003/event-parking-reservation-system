import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { EventCategory } from '../../models/category.model';
import { CategoryApiService } from '../../services/category-api.service';
import { CategoryManagement } from './category-management';

describe('CategoryManagement', () => {
  const categories: EventCategory[] = [
    {
      id: 1,
      name: 'Concert',
      createdAtUtc: '2026-09-01T00:00:00Z',
      updatedAtUtc: '2026-09-01T00:00:00Z'
    }
  ];

  let createdRequest: unknown;
  let deletedCategoryId: number | null;

  const categoryApiMock = {
    getAll: () => of(categories),

    create: (request: unknown) => {
      createdRequest = request;

      return of({
        ...categories[0],
        ...(request as object)
      });
    },

    update: (_categoryId: number, request: unknown) =>
      of({
        ...categories[0],
        ...(request as object)
      }),

    delete: (categoryId: number) => {
      deletedCategoryId = categoryId;
      return of(void 0);
    }
  };

  beforeEach(async () => {
    createdRequest = null;
    deletedCategoryId = null;

    await TestBed.configureTestingModule({
      imports: [CategoryManagement],
      providers: [
        {
          provide: CategoryApiService,
          useValue: categoryApiMock
        }
      ]
    }).compileComponents();
  });

  it('should create the component and load categories', () => {
    const fixture = TestBed.createComponent(CategoryManagement);
    const component = fixture.componentInstance;

    expect(component).toBeTruthy();
    expect(component.categories()).toEqual(categories);
    expect(component.loading()).toBe(false);
  });

  it('should trim category name before create', () => {
    const fixture = TestBed.createComponent(CategoryManagement);
    const component = fixture.componentInstance;

    component.categoryForm.setValue({
      name: '  Concert  '
    });

    component.submit();

    expect(createdRequest).toEqual({
      name: 'Concert'
    });
  });

  it('should reject a whitespace-only category name', () => {
    const fixture = TestBed.createComponent(CategoryManagement);
    const component = fixture.componentInstance;

    component.categoryForm.setValue({
      name: '   '
    });

    component.submit();

    expect(createdRequest).toBeNull();
    expect(component.categoryForm.controls.name.invalid).toBe(true);
  });

  it('should require confirmation before deleting a category', () => {
    const fixture = TestBed.createComponent(CategoryManagement);
    const component = fixture.componentInstance;

    component.deleteCategory(categories[0]);

    expect(component.pendingDeleteCategory()).toEqual(categories[0]);
    expect(deletedCategoryId).toBeNull();

    component.confirmDeleteCategory();

    expect(deletedCategoryId).toBe(1);
    expect(component.pendingDeleteCategory()).toBeNull();
  });
});

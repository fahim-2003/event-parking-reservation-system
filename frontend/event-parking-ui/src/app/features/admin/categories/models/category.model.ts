export interface EventCategory {
  id: number;
  name: string;
  createdAtUtc: string;
  updatedAtUtc: string;
}

export interface CreateCategoryRequest {
  name: string;
}

export interface UpdateCategoryRequest {
  name: string;
}
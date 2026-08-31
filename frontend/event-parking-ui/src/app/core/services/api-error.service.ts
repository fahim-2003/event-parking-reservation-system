import { HttpErrorResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ProblemDetails } from '../models/problem-details.model';

@Injectable({
  providedIn: 'root'
})
export class ApiErrorService {
  toProblemDetails(error: unknown): ProblemDetails {
    if (error instanceof HttpErrorResponse) {
      const body = error.error;

      if (this.isProblemDetails(body)) {
        return {
          ...body,
          status: body.status ?? error.status
        };
      }

      return {
        title: 'Request failed',
        status: error.status,
        detail: this.getFallbackMessage(error),
        instance: error.url ?? undefined
      };
    }

    return {
      title: 'Unexpected error',
      detail: 'An unexpected error occurred. Please try again.'
    };
  }

  getMessage(error: unknown): string {
    const problem = this.toProblemDetails(error);

    return (
      problem.detail ??
      problem.title ??
      'The request could not be completed.'
    );
  }

  isConflict(error: unknown): boolean {
    return error instanceof HttpErrorResponse && error.status === 409;
  }

  isValidationError(error: unknown): boolean {
    return error instanceof HttpErrorResponse && error.status === 400;
  }

  private isProblemDetails(value: unknown): value is ProblemDetails {
    return (
      typeof value === 'object' &&
      value !== null &&
      ('title' in value ||
        'detail' in value ||
        'status' in value ||
        'errors' in value)
    );
  }

  private getFallbackMessage(error: HttpErrorResponse): string {
    if (error.status === 0) {
      return 'Unable to connect to the server.';
    }

    if (error.status === 401) {
      return 'Authentication is required.';
    }

    if (error.status === 403) {
      return 'You do not have permission to perform this action.';
    }

    if (error.status === 404) {
      return 'The requested resource was not found.';
    }

    if (error.status === 409) {
      return 'The request conflicts with the current system state.';
    }

    return 'The server could not complete the request.';
  }
}
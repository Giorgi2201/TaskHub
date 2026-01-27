import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, catchError, of } from 'rxjs';

export interface Request {
  requestID?: number;
  category: string;
  subcategory: string;
  description: string;
  statusID: number;
  initiatorID: number;
  initiatorName?: string;
  createdAt?: string;
}

export interface RequestDetail {
  id: string;
  category: string;
  subcategory: string;
  description: string;
  createdDate: string;
  updatedDate: string;
  status: string;
  statusClass: string;
  submitter: {
    name: string;
    role: string;
    initials: string;
    roleLabel: string;
    avatarClass: string;
  };
  participants: Array<{
    name: string;
    role: string;
    initials: string;
    roleLabel: string;
    avatarClass: string;
  }>;
  comments: Array<{
    author: string;
    initials: string;
    date: string;
    text: string;
  }>;
}

export interface Category {
  categoryID: number;
  categoryName: string;
  subcategories: Array<{
    subcategoryID: number;
    subcategoryName: string;
  }>;
}

export interface Executor {
  userID: number;
  name: string;
  initials: string;
  department: string;
}

@Injectable({
  providedIn: 'root'
})
export class RequestService {
  private apiUrl = 'http://localhost:5250/api';

  constructor(private http: HttpClient) { }

  getRequests(): Observable<Request[]> {
    return this.http.get<Request[]>(`${this.apiUrl}/requests`).pipe(
      catchError(error => {
        console.error('API Error:', error);
        return of([]);
      })
    );
  }

  getRequest(id: number): Observable<RequestDetail> {
    return this.http.get<RequestDetail>(`${this.apiUrl}/requests/${id}`).pipe(
      catchError(error => {
        console.error('API Error:', error);
        throw error;
      })
    );
  }

  createRequest(request: Request): Observable<Request> {
    return this.http.post<Request>(`${this.apiUrl}/requests`, request);
  }

  getCategories(): Observable<Category[]> {
    return this.http.get<Category[]>(`${this.apiUrl}/categories`).pipe(
      catchError(error => {
        console.error('API Error:', error);
        return of([]);
      })
    );
  }

  approveRequest(requestId: number, userId: number, comment: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/requests/${requestId}/approve`, { userId, comment });
  }

  rejectRequest(requestId: number, userId: number, comment: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/requests/${requestId}/reject`, { userId, comment });
  }

  updateRequestStatus(
    requestId: number, 
    statusId: number, 
    executorId: number | null, 
    supervisorId: number,
    comment: string
  ): Observable<any> {
    return this.http.post(`${this.apiUrl}/requests/${requestId}/update`, {
      statusId,
      executorId,
      supervisorId,
      comment
    });
  }

  getExecutors(): Observable<Executor[]> {
    return this.http.get<Executor[]>(`${this.apiUrl}/requests/executors`).pipe(
      catchError(error => {
        console.error('API Error:', error);
        return of([]);
      })
    );
  }
}
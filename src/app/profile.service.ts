import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

// Read-only info shown on the profile page. Mirrors ProfileDto on the backend.
export interface ProfileInfo {
  name: string;
  email: string;
  title: string;
  department: string;
  role: string;
}

export interface ChangePasswordRequest {
  oldPassword: string;
  newPassword: string;
  confirmNewPassword: string;
}

// Thin HTTP wrapper only — no shared/global state needed here (unlike
// DraftService), since the profile modal doesn't need to persist across
// refreshes or be controlled from outside AppComponent.
@Injectable({ providedIn: 'root' })
export class ProfileService {
  private apiUrl = 'http://localhost:5250/api/profile';

  constructor(private http: HttpClient) {}

  getProfile(userId: number): Observable<ProfileInfo> {
    return this.http.get<ProfileInfo>(`${this.apiUrl}/${userId}`);
  }

  changePassword(userId: number, request: ChangePasswordRequest): Observable<{ message: string }> {
    return this.http.put<{ message: string }>(`${this.apiUrl}/${userId}/password`, request);
  }
}

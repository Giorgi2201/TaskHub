// ============================================================
// PATCH: user.service.ts  (or equivalent HTTP service file)
// Add these 4 methods inside your Angular service class.
// Replace the apiUrl base if yours is different.
// ============================================================

// Assumes: private apiUrl = 'http://localhost:PORT/api';  (already exists in your service)
// Assumes: constructor(private http: HttpClient) {}        (already exists in your service)

getDigestEntries(activeOnly: boolean = false): Observable<any[]> {
  const url = activeOnly
    ? `${this.apiUrl}/digest?activeOnly=true`
    : `${this.apiUrl}/digest`;
  return this.http.get<any[]>(url);
}

createDigestEntry(data: any): Observable<any> {
  return this.http.post<any>(`${this.apiUrl}/digest`, data);
}

updateDigestEntry(id: number, data: any): Observable<void> {
  return this.http.put<void>(`${this.apiUrl}/digest/${id}`, data);
}

deleteDigestEntry(id: number): Observable<void> {
  return this.http.delete<void>(`${this.apiUrl}/digest/${id}`);
}

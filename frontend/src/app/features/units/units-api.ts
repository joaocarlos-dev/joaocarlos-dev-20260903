import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { CreateUnitRequest, Unit, UpdateUnitRequest } from './unit.models';

@Injectable({ providedIn: 'root' })
export class UnitsApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/v1/units';

  list(): Observable<readonly Unit[]> {
    return this.http.get<readonly Unit[]>(this.baseUrl);
  }

  get(id: string): Observable<Unit> {
    return this.http.get<Unit>(`${this.baseUrl}/${id}`);
  }

  create(request: CreateUnitRequest): Observable<string> {
    return this.http.post<string>(this.baseUrl, request);
  }

  update(id: string, request: UpdateUnitRequest): Observable<void> {
    return this.http.patch<void>(`${this.baseUrl}/${id}`, request);
  }
}

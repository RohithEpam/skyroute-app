import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Airport } from '../models/airport.model';
import { Flight } from '../models/flight.model';
import { BookingRequest, BookingResponse } from '../models/booking.model';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class FlightService {
  private baseUrl = environment.apiBaseUrl;

  constructor(private http: HttpClient) {}

  getAirports(): Observable<Airport[]> {
    return this.http.get<Airport[]>(`${this.baseUrl}/flights/airports`);
  }

  searchFlights(payload: {
    originAirportId: number;
    destinationAirportId: number;
    departureDate: string;
    passengers: number;
    cabinClass: string;
  }): Observable<Flight[]> {
    return this.http.post<Flight[]>(`${this.baseUrl}/flights/search`, payload);
  }

  bookFlight(request: BookingRequest): Observable<BookingResponse> {
  return this.http.post<BookingResponse>(`${this.baseUrl}/bookings`, request);
  }

  getBookingById(id: number): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/bookings?id=${id}`);
  }
}
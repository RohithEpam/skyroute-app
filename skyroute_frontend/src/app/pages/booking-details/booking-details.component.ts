import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { FlightService } from '../../services/flight.service';

@Component({
  selector: 'app-booking-details',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './booking-details.component.html',
  styleUrl: './booking-details.component.scss'
})
export class BookingDetailsComponent {
  form: FormGroup;
  booking: any = null;
  loading = false;
  error = '';
  searched = false;

  constructor(
    private fb: FormBuilder,
    private flightService: FlightService,
    private router: Router
  ) {
    const state = this.router.getCurrentNavigation()?.extras.state as any;
    const bookingId = state?.bookingId || null;
    
    this.form = this.fb.group({
      bookingId: [bookingId, [Validators.required, Validators.min(1)]]
    });
    
    // auto-fetch immediately if bookingId was passed
    if (bookingId) {
      setTimeout(() => this.fetch(), 0);
    }
  }

  getDuration(dep: string, arr: string): string {
    const diff = new Date(arr).getTime() - new Date(dep).getTime();
    const h = Math.floor(diff / 3600000);
    const m = Math.floor((diff % 3600000) / 60000);
    return `${h}h ${m}m`;
  }

  fetch(): void {
    if (this.form.invalid) return;
    this.loading = true;
    this.error = '';
    this.booking = null;
    this.searched = true;

    const id = this.form.value.bookingId;
    this.flightService.getBookingById(id).subscribe({
      next: (data) => {
        // If backend returns an array, pick the first element
        let booking = Array.isArray(data) ? data[0] : data;
        if (!booking) {
          this.error = `No booking found with ID #${id}.`;
          this.loading = false;
          return;
        }
        // Patch missing fields for template compatibility
        booking.id = booking.id || booking.flight?.id || id;
        booking.airLine = booking.airLine || booking.flight?.provider || '';
        booking.bookingDate = booking.bookingDate || new Date();
        this.booking = booking;
        this.loading = false;
      },
      error: (err) => {
        this.error = err.status === 404 || err.status === 500
          ? `No booking found with ID #${id}.`
          : 'Something went wrong. Please try again.';
        this.loading = false;
      }
    });
  }

  goHome(): void {
    this.router.navigate(['/']);
  }
}
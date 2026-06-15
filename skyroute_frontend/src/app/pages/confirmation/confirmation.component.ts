import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { Flight } from '../../models/flight.model';

@Component({
  selector: 'app-confirmation',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './confirmation.component.html',
  styleUrl: './confirmation.component.scss'
})
export class ConfirmationComponent implements OnInit {
  booking: any;
  flight!: Flight;
  passengerName = '';

  constructor(private router: Router) {
    const state = this.router.getCurrentNavigation()?.extras.state as any;
    if (state) {
      this.booking = state.booking;
      this.flight = state.flight;
      this.passengerName = state.passengerName;
    }
  }

  ngOnInit(): void {
    if (!this.booking) this.router.navigate(['/']);
  }

  searchAgain(): void {
    this.router.navigate(['/']);
  }

  viewBooking(): void {
    this.router.navigate(['/my-booking'], {
        state: { bookingId: this.booking?.id }
    });
}
}
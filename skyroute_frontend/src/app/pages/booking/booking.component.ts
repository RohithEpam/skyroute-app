import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, FormArray, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FlightService } from '../../services/flight.service';
import { Flight } from '../../models/flight.model';

@Component({
  selector: 'app-booking',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './booking.component.html',
  styleUrl: './booking.component.scss'
})
export class BookingComponent implements OnInit {
  form: FormGroup;
  flight!: Flight;
  passengers = 1;
  loading = false;
  error = '';

  constructor(
    private fb: FormBuilder,
    private flightService: FlightService,
    private router: Router
  ) {
    const state = this.router.getCurrentNavigation()?.extras.state as any;
    if (state) {
      this.flight = state.flight;
      this.passengers = state.passengers;
    }

    this.form = this.fb.group({
      customerDetails: this.fb.array([])
    });

    // Build one form group per passenger
    this.buildPassengerForms();
  }

  ngOnInit(): void {
    if (!this.flight) this.router.navigate(['/']);
  }

  // Getter for easy access in template
  get customerDetails(): FormArray {
    return this.form.get('customerDetails') as FormArray;
  }

  // Returns a single passenger form group
  private createPassengerGroup(): FormGroup {
    return this.fb.group({
      fullName:       ['', [Validators.required, Validators.minLength(2)]],
      email:          ['', [Validators.required, Validators.email]],
      documentNumber: ['', [Validators.required, Validators.minLength(5), Validators.maxLength(50)]],
      phoneNumber:    ['', [Validators.required, Validators.minLength(5), Validators.maxLength(20)]],
    });
  }

  // Build N forms based on passenger count
  private buildPassengerForms(): void {
    for (let i = 0; i < this.passengers; i++) {
      this.customerDetails.push(this.createPassengerGroup());
    }
  }

  getDuration(dep: string, arr: string): string {
    const diff = new Date(arr).getTime() - new Date(dep).getTime();
    const h = Math.floor(diff / 3600000);
    const m = Math.floor((diff % 3600000) / 60000);
    return `${h}h ${m}m`;
  }

  isInvalid(groupIndex: number, field: string): boolean {
    const control = this.customerDetails.at(groupIndex).get(field);
    return !!(control?.invalid && control?.touched);
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.loading = true;
    this.error = '';

    const payload = {
      flightId: this.flight.id,
      airLine: this.flight.provider,
      customerDetails: this.customerDetails.value  // array of passengers
    };

    this.flightService.bookFlight(payload).subscribe({
      next: (res) => {
        this.loading = false;
        this.router.navigate(['/confirmation'], {
          state: {
            booking: res,
            flight: this.flight,
            passengers: this.passengers,
            passengerName: this.customerDetails.at(0).value.fullName
          }
        });
      },
      error: () => {
        this.loading = false;
        this.error = 'Booking failed. Please check your details and try again.';
      }
    });
  }

  goBack(): void {
    window.history.back();
  }
}
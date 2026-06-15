import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FlightService } from '../../services/flight.service';
import { Airport } from '../../models/airport.model';

@Component({
  selector: 'app-search',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './search.component.html',
  styleUrl: './search.component.scss'
})
export class SearchComponent implements OnInit {
  form: FormGroup;
  airports: Airport[] = [];
  loading = false;
  searching = false;
  error = '';

  cabinClasses = ['Economy', 'Business', 'First'];

  constructor(
    private fb: FormBuilder,
    private flightService: FlightService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {
    this.form = this.fb.group({
      originAirportId: [null, Validators.required],
      destinationAirportId: [null, Validators.required],
      departureDate: ['', Validators.required],
      passengers: [1, [Validators.required, Validators.min(1), Validators.max(9)]],
      cabinClass: ['Economy', Validators.required]
    });
  }

  ngOnInit(): void {
    this.loading = true;
    this.flightService.getAirports().subscribe({
      next: (data) => { this.airports = data;  this.loading = false; },
      error: () => { this.error = 'Could not load airports. Is the backend running?'; this.loading = false; }
    });
    this.cdr.detectChanges();
  }

  get today(): string {
    return new Date().toISOString().split('T')[0];
  }

  get sameAirport(): boolean {
    const { originAirportId, destinationAirportId } = this.form.value;
    return originAirportId && destinationAirportId && originAirportId === destinationAirportId;
  }

  search(): void {
    if (this.form.invalid || this.sameAirport) return;
    this.searching = true;
    this.error = '';
    const v = this.form.value;

    this.flightService.searchFlights({
      originAirportId: +v.originAirportId,
      destinationAirportId: +v.destinationAirportId,
      departureDate: new Date(v.departureDate).toISOString(),
      passengers: v.passengers,
      cabinClass: v.cabinClass
    }).subscribe({
      next: (flights) => {
        this.searching = false;
        this.router.navigate(['/results'], {
          state: { flights, passengers: v.passengers, cabinClass: v.cabinClass }
        });
      },
      error: () => {
        this.searching = false;
        this.error = 'Search failed. Please try again.';
      }
    });
  }
}
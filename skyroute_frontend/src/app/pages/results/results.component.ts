import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule, DatePipe, DecimalPipe } from '@angular/common';
import { Flight } from '../../models/flight.model';

@Component({
  selector: 'app-results',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './results.component.html',
  styleUrl: './results.component.scss'
})
export class ResultsComponent implements OnInit {
  flights: Flight[] = [];
  passengers = 1;
  cabinClass = '';

  constructor(private router: Router) {
    const state = this.router.getCurrentNavigation()?.extras.state as any;
    if (state) {
      this.flights = state.flights;
      this.passengers = state.passengers;
      this.cabinClass = state.cabinClass;
    }
  }

  ngOnInit(): void {
    if (!this.flights?.length && this.flights !== null) {
      // allow empty results, but redirect if no state at all
    }
  }

  getDuration(dep: string, arr: string): string {
    const diff = new Date(arr).getTime() - new Date(dep).getTime();
    const h = Math.floor(diff / 3600000);
    const m = Math.floor((diff % 3600000) / 60000);
    return `${h}h ${m}m`;
  }

  selectFlight(flight: Flight): void {
    this.router.navigate(['/booking'], {
      state: { flight, passengers: this.passengers }
    });
  }

  goBack(): void {
    this.router.navigate(['/']);
  }
}
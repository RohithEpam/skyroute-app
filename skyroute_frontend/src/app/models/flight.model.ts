export interface Flight {
  id: number;
  provider: string;
  flightNumber: string;
  departureTime: string;
  arrivalTime: string;
  cabinClass: string;
  pricePerPassenger: number;
  totalPrice: number;
}
export interface CustomerDetail {
  fullName: string;
  email: string;
  documentNumber: string;
  phoneNumber: string;
}

export interface BookingRequest {
  flightId: number;
  airLine: string;
  customerDetails: CustomerDetail[];  
}

export interface BookingResponse {
  id: number;
  referenceCode: string;
}
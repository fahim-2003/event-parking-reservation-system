export interface AdminDashboard {
  totalEvents: number;
  totalBookings: number;
  availableSeats: number;
  occupiedParkingSlots: number;
  totalRevenue: number;
  totalCustomers: number;
}


export interface CustomerDashboard {
  upcomingBookings: number;
  reservedParking: number;
  recentPayments: number;
  unreadNotifications: number;
}

# Hidden inputs

Line numbers refer to the original code.

## Fixed

| Class | Line | Hidden input | Why it hurts a test | How it was removed |
| --- | --- | --- | --- | --- |
| BookingService | 56, 89, 125 | `DateTime.Now` | Result depends on the moment the test runs | Constructor takes `TimeProvider` |
| BookingService | 150-158 | `new Random()` in `RandomCode` | Booking reference is not predictable | Constructor takes `IReferenceGenerator` |
| CancellationService | 25, 33 | `DateTime.Now` | Fee depends on the current time | Constructor takes `TimeProvider` |
| ReturnService | 37 | `DateTime.Now` | Late fee depends on the current time | Constructor takes `TimeProvider` |
| PricingService | 77 | `DateTime.Today` (twice) | Promo validity depends on today | Constructor takes `TimeProvider` |
| PaymentGateway | 7, 18-23, 25-33 | `Thread.Sleep(1500)` and `new Random()` | Slow test that fails 1 time in 10 | Constructor takes `Random` and the delay |
| SmsSender | 7 | `Thread.Sleep(500)` | Slow test | Constructor takes the delay |
| EmailSender | 9 | `DateTime.Now` in the file name | File name is not predictable | Constructor takes `TimeProvider` |
| InMemoryDatabase | 19-126 | `DateTime.Today` in seed data | Seed data moves with the real date | `Reset(TimeProvider)` seeds the data |
| Program (ConsoleApp) | 101 | `DateTime.Today` | Age shown depends on the real date | Uses the `TimeProvider` created in ConsoleApp |

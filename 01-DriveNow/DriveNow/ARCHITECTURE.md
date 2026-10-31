# Failed Tests

## R2 - Application dependencies
- Failed
- `DriveNow.Application` has a dependency on `DriveNow.Infrastructure`.
- when i tried to derefrence infrastructure .. i got build error


## R4 - Production naming conventions
- Failed
- `DriveNow.Infrastructure.Repositories.CustomerRepo` does not end with `Repository`, despite being in the `Repositories` namespace.
- make it harder to debug


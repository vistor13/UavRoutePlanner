namespace UavRoutePlanner.BLL.Messages;

public static class Messages
{
    public static class Error
    {
        public const string UserAlreadyExist =
            "A user with this name already exists.";

        public const string InvalidCredentials =
            "Invalid username or password.";

        public const string UserNameLength =
            "The username must be between 3 and 50 characters long.";

        public const string PasswordTooShort =
            "The password must be at least 6 characters long.";

        public const string TooManyLoginAttempts =
            "Too many failed sign-in attempts. Try again in {0} minutes.";

        public const string TooManyRequests =
            "Too many requests. Try again in a minute.";

        public const string UavProfileNotFound =
            "UAV profile not found.";

        public const string UavProfileName =
            "The profile name must be between 1 and 100 characters long.";

        public const string MaxFlightTimeNotPositive =
            "The maximum flight time must be greater than zero.";

        public const string ReserveOutOfRange =
            "The battery reserve must be from 0% to 100% (100 excluded).";

        public const string AltitudeOutOfRange =
            "The flight altitude must be between 0 and 180 m (wind data is available up to 180 m).";

        public const string AirspeedNotPositive =
            "The UAV airspeed must be greater than zero.";

        public const string WindSpeedNegative =
            "The wind speed cannot be negative.";

        public const string WindExceedsAirspeed =
            "The wind speed is not less than the UAV airspeed: the flight along the given route is impossible.";

        public const string PointsCoincide =
            "The start and end points of the leg coincide.";

        public const string MissionNotFound =
            "Mission not found.";

        public const string MissionName =
            "The mission name must be between 1 and 200 characters long.";

        public const string MissionPointsMissing =
            "The start and finish points must be specified.";

        public const string MissionObjectsCount =
            "The number of survey objects must be from 1 to 100.";

        public const string CoordinatesOutOfRange =
            "The coordinates are out of range: latitude from -90 to 90, longitude from -180 to 180.";

        public const string ObjectNameTooLong =
            "The object name cannot be longer than 200 characters.";

        public const string ObjectsCoincide =
            "Objects cannot coincide with each other or with the start or finish point (distance less than 1 m).";

        public const string AlgorithmParameters =
            "The algorithm parameters are out of range: α ≥ 0, β ≥ 0, 0 < ρ < 1, Q > 0, number of ants from 1 to 500, iterations from 1 to 5000.";

        public const string AlgorithmTooHeavy =
            "The computation is too large: the product of the number of ants, iterations and the squared number of points exceeds the allowed value. Reduce the number of ants or iterations.";

        public const string TimeBudgetTooSmall =
            "The UAV time budget is not enough even for a direct flight from the start to the finish.";

        public const string DepartureOutsideForecast =
            "There is no wind forecast for the specified departure hour. Choose an hour within the next few days.";

        public const string ManualWindInvalid =
            "The wind is set incorrectly: the speed must not be negative, the direction must be from 0° to 360°.";

        public const string DepartureHoursOutOfRange =
            "The number of hours to review must be from 1 to 72.";

        public const string NoFeasibleDeparture =
            "The flight is impossible in any of the reviewed hours: the wind is too strong or the time budget is insufficient.";

        public const string ExportFormatUnknown =
            "Unknown export format. Available: csv, geojson, waypoints.";

        public const string ImportEmpty =
            "The file is empty.";

        public const string ImportTooLarge =
            "The file is too large: up to 1 MB is allowed.";

        public const string ImportFormatUnsupported =
            "Unsupported file format. CSV and GeoJSON are supported.";

        public const string ImportInvalidJson =
            "The file is not valid JSON.";

        public const string ImportNotGeoJson =
            "The file does not look like GeoJSON: the type field is missing.";

        public const string ImportNoObjects =
            "No objects were found in the file.";

        public const string ImportHeaderUnknown =
            "Could not determine the latitude and longitude columns in the header. Expected names: lat/latitude and lon/lng/longitude.";

        public const string ImportRowTooShort =
            "The row does not have enough columns.";

        public const string ImportNotANumber =
            "Latitude and longitude must be numbers.";

        public const string ImportCoordinatesOutOfRange =
            "The coordinates are out of range: latitude from -90 to 90, longitude from -180 to 180.";

        public const string ImportCoordinatesSwapped =
            "Latitude and longitude seem to be swapped: in CSV the latitude comes first, then the longitude.";

        public const string ImportNameTooLong =
            "The object name cannot be longer than 200 characters.";

        public const string ImportTooManyObjects =
            "More than 100 objects: the rest were ignored.";

        public const string ImportDuplicate =
            "The object coincides with the object from row {0} (distance less than 1 m).";

        public const string ImportNoGeometry =
            "The element has no geometry with coordinates.";

        public const string ImportGeometryUnsupported =
            "The geometry type {0} is not supported: points are required (Point, MultiPoint).";

        public const string RouteNotFound =
            "Route not found.";

        public const string WeatherServiceUnavailable =
            "Could not get the wind forecast from the Open-Meteo service. Try again later.";
    }
}

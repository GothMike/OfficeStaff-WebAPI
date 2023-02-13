﻿using OfficeStaff.Data.Models;

namespace OfficeStaff.Data.Interfaces
{
    public interface ICountryRepository
    {
        bool CreateCountry(Country country);
        Country GetCountry(int countryId);
        ICollection<Country> GetCountries();
        bool UpdateCountry(Country country);
        bool DeleteCountry(Country country);
        bool CountryExists(int countryId);
        bool Save();
    }
}
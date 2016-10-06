using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TietoGadget.Models
{
    public class User
    {        
        [Required]        
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }

        public string Salutation { get; set; }
       
        public string Prefix { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Surname { get; set; }
         
        public string Telephone { get; set; }
        
        public string Company { get; set; }       
        [Required]
        public string City { get; set; }
        [Required]
        public string State { get; set; }
        [Required]
        public string Zip { get; set; }
        [Required]
        private DateTime _birthdate;
        public DateTime Birthdate
        {
            get { return _birthdate; }
            set { _birthdate = Convert.ToDateTime(value); }
        }        
        public int Age { get; set; }
        [Required]
        public decimal Working_years { get; set; }

        public List<Subscription_Category> Subscription_Categories;

        private List<Subscription_Category> _userPreferences;
        public List<Subscription_Category> UserPreferences
        {
            get { return _userPreferences ?? (_userPreferences = new List<Subscription_Category>()); }
            set { _userPreferences = value; }
        }

    }

    public class Subscription_Category
    {
        public string TcmId { set; get; }
        public string Category { set; get; }
        public bool IsChecked { get; set; }
    }
}
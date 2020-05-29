﻿using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Text;

namespace BulkyBook.Models.ViewModels
{
    public class ProductVM
    {
        public Product Product { get; set; }
        public  IEnumerable<SelectListItem> CategorieList{ get; set; }
        public  IEnumerable<SelectListItem> CoverTypesList {get; set; }
       
    }
}
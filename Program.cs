using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using FinalProject.Model;
using Microsoft.EntityFrameworkCore;

namespace FinalProject
{
    class Program
    {
        static void Main(string[] args)
        {
            string path = Directory.GetCurrentDirectory() + "\\nlog.config";

            // create instance of Logger
            var logger = LogManager.LoadConfiguration(path).GetCurrentClassLogger();
            logger.Info("Program started");

            try
            {
                var db = new NWContext();
                string choice;
                do
                {
                    Console.WriteLine("1) Display Categories");
                    Console.WriteLine("2) Add Category");
                    Console.WriteLine("3) Display Category and related products");
                    Console.WriteLine("4) Display all Categories and their related products");
                    Console.WriteLine("5) Add new record to Products table");
                    Console.WriteLine("6) Edit a specified record from the Products table");
                    Console.WriteLine("7) Display all records in the Products table (ProductName only)");
                    Console.WriteLine("8) Display a specific Product");
                    Console.WriteLine("9) Delete a specified existing record from the Products table");
                    Console.WriteLine("10) Delete a specified existing record from the Categories table");
                    Console.WriteLine("11) Calculate the total value of all products in stock");
                    Console.WriteLine("\"q\" to quit");
                    choice = Console.ReadLine();
                    Console.Clear();
                    logger.Info($"Option {choice} selected");

                    if (choice == "1")
                    {
                        DisplayCategories(db, logger);
                    }
                    else if (choice == "2")
                    {
                        AddCategory(db, logger);
                    }
                    else if (choice == "3")
                    {
                        DisplayCategoryAndProducts(db, logger);
                    }
                    else if (choice == "4")
                    {
                        DisplayAllCategoriesAndProducts(db, logger);
                    }
                    else if (choice == "5")
                    {
                        AddProduct(db, logger);
                    }
                    else if (choice == "6")
                    {
                        EditProduct(db, logger);
                    }
                    else if (choice == "7")
                    {
                        DisplayAllProducts(db, logger);
                    }
                    else if (choice == "8")
                    {
                        DisplaySpecificProduct(db, logger);
                    }
                    else if (choice == "9")
                    {
                        DeleteProduct(db, logger);
                    }
                    else if (choice == "10")
                    {
                        DeleteCategory(db, logger);
                    }
                    else if (choice == "11")
                    {
                        CalculateTotalValueInStock(db, logger);
                    }

                    Console.WriteLine();
                } while (choice.ToLower() != "q");
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }

            logger.Info("Program ended");
        }

        static void DisplayCategories(NWContext db, Logger logger)
        {
            var query = db.Categories.OrderBy(p => p.CategoryName);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"{query.Count()} records returned");
            Console.ForegroundColor = ConsoleColor.Magenta;
            foreach (var item in query)
            {
                Console.WriteLine($"{item.CategoryName} - {item.Description}");
            }
            Console.ForegroundColor = ConsoleColor.White;
        }

        static void AddCategory(NWContext db, Logger logger)
        {
            Category category = new Category();
            Console.WriteLine("Enter Category Name:");
            category.CategoryName = Console.ReadLine();
            Console.WriteLine("Enter the Category Description:");
            category.Description = Console.ReadLine();
            ValidationContext context = new ValidationContext(category, null, null);
            List<ValidationResult> results = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(category, context, results, true);
            if (isValid)
            {
                // check for unique name
                if (db.Categories.Any(c => c.CategoryName == category.CategoryName))
                {
                    // generate validation error
                    isValid = false;
                    results.Add(new ValidationResult("Name exists", new string[] { "CategoryName" }));
                }
                else
                {
                    logger.Info("Validation passed");
                    db.Categories.Add(category);
                    db.SaveChanges();
                    logger.Info("Category added to database");
                }
            }
            if (!isValid)
            {
                foreach (var result in results)
                {
                    logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
                }
            }
        }

        static void DisplayCategoryAndProducts(NWContext db, Logger logger)
        {
            var query = db.Categories.OrderBy(p => p.CategoryId);

            Console.WriteLine("Select the category whose products you want to display:");
            Console.ForegroundColor = ConsoleColor.DarkRed;
            foreach (var item in query)
            {
                Console.WriteLine($"{item.CategoryId}) {item.CategoryName}");
            }
            Console.ForegroundColor = ConsoleColor.White;
            int id = int.Parse(Console.ReadLine());
            Console.Clear();
            logger.Info($"CategoryId {id} selected");
            Category category = db.Categories.Include("Products").FirstOrDefault(c => c.CategoryId == id);
            Console.WriteLine($"{category.CategoryName} - {category.Description}");
            foreach (Product p in category.Products)
            {
                Console.WriteLine($"\t{p.ProductName}");
            }
        }

        static void DisplayAllCategoriesAndProducts(NWContext db, Logger logger)
        {
            var query = db.Categories.Include("Products").OrderBy(p => p.CategoryId);
            foreach (var item in query)
            {
                Console.WriteLine($"{item.CategoryName}");
                foreach (Product p in item.Products)
                {
                    Console.WriteLine($"\t{p.ProductName}");
                }
            }
        }

        static void AddProduct(NWContext db, Logger logger)
        {
            Product product = new Product();
            Console.WriteLine("Enter Product Name:");
            product.ProductName = Console.ReadLine();
            Console.WriteLine("Enter the Supplier ID:");
            product.SupplierId = int.Parse(Console.ReadLine());
            Console.WriteLine("Enter the Category ID:");
            product.CategoryId = int.Parse(Console.ReadLine());
            Console.WriteLine("Enter the Quantity Per Unit:");
            product.QuantityPerUnit = Console.ReadLine();
            Console.WriteLine("Enter the Unit Price:");
            product.UnitPrice = decimal.Parse(Console.ReadLine());
            Console.WriteLine("Enter the Units In Stock:");
            product.UnitsInStock = short.Parse(Console.ReadLine());
            Console.WriteLine("Enter the Units On Order:");
            product.UnitsOnOrder = short.Parse(Console.ReadLine());
            Console.WriteLine("Enter the Reorder Level:");
            product.ReorderLevel = short.Parse(Console.ReadLine());
            Console.WriteLine("Is the product discontinued? (true/false):");
            product.Discontinued = bool.Parse(Console.ReadLine());

            db.Products.Add(product);
            db.SaveChanges();
            logger.Info("Product added to database");
        }

        static void EditProduct(NWContext db, Logger logger)
        {
            Console.WriteLine("Enter the Product ID to edit:");
            int id = int.Parse(Console.ReadLine());
            var product = db.Products.Find(id);
            if (product != null)
            {
                Console.WriteLine("Enter Product Name:");
                product.ProductName = Console.ReadLine();
                Console.WriteLine("Enter the Supplier ID:");
                product.SupplierId = int.Parse(Console.ReadLine());
                Console.WriteLine("Enter the Category ID:");
                product.CategoryId = int.Parse(Console.ReadLine());
                Console.WriteLine("Enter the Quantity Per Unit:");
                product.QuantityPerUnit = Console.ReadLine();
                Console.WriteLine("Enter the Unit Price:");
                product.UnitPrice = decimal.Parse(Console.ReadLine());
                Console.WriteLine("Enter the Units In Stock:");
                product.UnitsInStock = short.Parse(Console.ReadLine());
                Console.WriteLine("Enter the Units On Order:");
                product.UnitsOnOrder = short.Parse(Console.ReadLine());
                Console.WriteLine("Enter the Reorder Level:");
                product.ReorderLevel = short.Parse(Console.ReadLine());
                Console.WriteLine("Is the product discontinued? (true/false):");
                product.Discontinued = bool.Parse(Console.ReadLine());

                db.SaveChanges();
                logger.Info($"Product with ID {id} updated");
            }
            else
            {
                Console.WriteLine("Product not found");
                logger.Warn($"Product with ID {id} not found");
            }
        }

        static void DisplayAllProducts(NWContext db, Logger logger)
        {
            Console.WriteLine("Do you want to see all products (A), discontinued products (D), or active products (not discontinued) (C)?");
            char choice = char.Parse(Console.ReadLine().ToUpper());
            var query = choice switch
            {
                'A' => db.Products.OrderBy(p => p.ProductName),
                'D' => db.Products.Where(p => p.Discontinued).OrderBy(p => p.ProductName),
                'C' => db.Products.Where(p => !p.Discontinued).OrderBy(p => p.ProductName),
                _ => throw new ArgumentException("Invalid choice")
            };

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"{query.Count()} records returned");
            Console.ForegroundColor = ConsoleColor.Magenta;
            foreach (var item in query)
            {
                Console.WriteLine($"{item.ProductName} - {(item.Discontinued ? "Discontinued" : "Active")}");
            }
            Console.ForegroundColor = ConsoleColor.White;
        }

        static void DisplaySpecificProduct(NWContext db, Logger logger)
        {
            Console.WriteLine("Enter the Product ID to display:");
            int id = int.Parse(Console.ReadLine());
            var product = db.Products.Find(id);
            if (product != null)
            {
                Console.WriteLine($"Product ID: {product.ProductId}");
                Console.WriteLine($"Product Name: {product.ProductName}");
                Console.WriteLine($"Supplier ID: {product.SupplierId}");
                Console.WriteLine($"Category ID: {product.CategoryId}");
                Console.WriteLine($"Quantity Per Unit: {product.QuantityPerUnit}");
                Console.WriteLine($"Unit Price: {product.UnitPrice}");
                Console.WriteLine($"Units In Stock: {product.UnitsInStock}");
                Console.WriteLine($"Units On Order: {product.UnitsOnOrder}");
                Console.WriteLine($"Reorder Level: {product.ReorderLevel}");
                Console.WriteLine($"Discontinued: {(product.Discontinued ? "Yes" : "No")}");
            }
            else
            {
                Console.WriteLine("Product not found");
                logger.Warn($"Product with ID {id} not found");
            }
        }

        static void DeleteProduct(NWContext db, Logger logger)
        {
            Console.WriteLine("Enter the Product ID to delete:");
            int id = int.Parse(Console.ReadLine());
            var product = db.Products.Find(id);
            if (product != null)
            {
                db.Products.Remove(product);
                db.SaveChanges();
                logger.Info($"Product with ID {id} deleted");
            }
            else
            {
                Console.WriteLine("Product not found");
                logger.Warn($"Product with ID {id} not found");
            }
        }

        static void DeleteCategory(NWContext db, Logger logger)
        {
            Console.WriteLine("Enter the Category ID to delete:");
            int id = int.Parse(Console.ReadLine());
            var category = db.Categories.Find(id);
            if (category != null)
            {
                // Remove all related products first
                var products = db.Products.Where(p => p.CategoryId == id);
                foreach (var product in products)
                {
                    db.Products.Remove(product);
                }

                db.Categories.Remove(category);
                db.SaveChanges();
                logger.Info($"Category with ID {id} deleted");
            }
            else
            {
                Console.WriteLine("Category not found");
                logger.Warn($"Category with ID {id} not found");
            }
        }

        static void CalculateTotalValueInStock(NWContext db, Logger logger)
        {
            decimal totalValue = db.Products.Sum(p => (decimal?)(p.UnitPrice * p.UnitsInStock)) ?? 0;
            Console.WriteLine($"Total value of all products in stock: {totalValue:C}");
            logger.Info($"Total value of all products in stock: {totalValue:C}");
        }
    }
}

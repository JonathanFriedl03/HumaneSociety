﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace HumaneSociety
{
    public static class Query
    {        
        public static HumaneSocietyDataContext db;
        public static EmployeeData employeeInfo;
        public delegate void EmployeeData(Employee employee);
        public static Employee employee;

        static Query()
        {
            db = new HumaneSocietyDataContext();
            employee = new Employee();
            
        }
        internal static List<USState> GetStates()
        {
            List<USState> allStates = db.USStates.ToList();       

            return allStates;
        }
            
        internal static Client GetClient(string userName, string password)
        {
            Client client = db.Clients.Where(c => c.UserName == userName && c.Password == password).Single();

            return client;
        }

        internal static List<Client> GetClients()
        {
            List<Client> allClients = db.Clients.ToList();

            return allClients;
        }

        internal static void AddNewClient(string firstName, string lastName, string username, string password, string email, string streetAddress, int zipCode, int stateId)
        {
            Client newClient = new Client();

            newClient.FirstName = firstName;
            newClient.LastName = lastName;
            newClient.UserName = username;
            newClient.Password = password;
            newClient.Email = email;

            Address addressFromDb = db.Addresses.Where(a => a.AddressLine1 == streetAddress && a.Zipcode == zipCode && a.USStateId == stateId).FirstOrDefault();

            // if the address isn't found in the Db, create and insert it
            if (addressFromDb == null)
            {
                Address newAddress = new Address();
                newAddress.AddressLine1 = streetAddress;
                newAddress.City = null;
                newAddress.USStateId = stateId;
                newAddress.Zipcode = zipCode;                

                db.Addresses.InsertOnSubmit(newAddress);
                db.SubmitChanges();

                addressFromDb = newAddress;
            }

            // attach AddressId to clientFromDb.AddressId
            newClient.AddressId = addressFromDb.AddressId;

            db.Clients.InsertOnSubmit(newClient);

            db.SubmitChanges();
        }

        internal static void UpdateClient(Client clientWithUpdates)
        {
            // find corresponding Client from Db
            Client clientFromDb = db.Clients.Where(c => c.ClientId == clientWithUpdates.ClientId).Single();

            // update clientFromDb information with the values on clientWithUpdates (aside from address)
            clientFromDb.FirstName = clientWithUpdates.FirstName;
            clientFromDb.LastName = clientWithUpdates.LastName;
            clientFromDb.UserName = clientWithUpdates.UserName;
            clientFromDb.Password = clientWithUpdates.Password;
            clientFromDb.Email = clientWithUpdates.Email;

            // get address object from clientWithUpdates
            Address clientAddress = clientWithUpdates.Address;

            // look for existing Address in Db (null will be returned if the address isn't already in the Db
            Address updatedAddress = db.Addresses.Where(a => a.AddressLine1 == clientAddress.AddressLine1 && a.USStateId == clientAddress.USStateId && a.Zipcode == clientAddress.Zipcode).FirstOrDefault();

            // if the address isn't found in the Db, create and insert it
            if(updatedAddress == null)
            {
                Address newAddress = new Address();
                newAddress.AddressLine1 = clientAddress.AddressLine1;
                newAddress.City = null;
                newAddress.USStateId = clientAddress.USStateId;
                newAddress.Zipcode = clientAddress.Zipcode;                

                db.Addresses.InsertOnSubmit(newAddress);
                db.SubmitChanges();

                updatedAddress = newAddress;
            }

            // attach AddressId to clientFromDb.AddressId
            clientFromDb.AddressId = updatedAddress.AddressId;
            
            // submit changes
            db.SubmitChanges();
        }
        
        internal static void AddUsernameAndPassword(Employee employee)
        {
            Employee employeeFromDb = db.Employees.Where(e => e.EmployeeId == employee.EmployeeId).FirstOrDefault();

            employeeFromDb.UserName = employee.UserName;
            employeeFromDb.Password = employee.Password;

            db.SubmitChanges();
        }

        internal static Employee RetrieveEmployeeUser(string email, int employeeNumber)
        {
            Employee employeeFromDb = db.Employees.Where(e => e.Email == email && e.EmployeeNumber == employeeNumber).FirstOrDefault();

            if (employeeFromDb == null)
            {
                throw new NullReferenceException();
            }
            else
            {
                return employeeFromDb;
            }
        }

        internal static Employee EmployeeLogin(string userName, string password)
        {
            Employee employeeFromDb = db.Employees.Where(e => e.UserName == userName && e.Password == password).FirstOrDefault();

            return employeeFromDb;
        }

        internal static bool CheckEmployeeUserNameExist(string userName)
        {
            Employee employeeWithUserName = db.Employees.Where(e => e.UserName == userName).FirstOrDefault();

            return employeeWithUserName == null;
        }


        //// TODO Items: ////
        
        // TODO: Allow any of the CRUD operations to occur here
        internal static void RunEmployeeQueries(Employee employee, string crudOperation)
        {
            
            EmployeeData employeeInfo = SetDelegate(crudOperation);
            employeeInfo(employee);
            
        }
        public static EmployeeData SetDelegate(string crudOperation)
        {
            switch (crudOperation)
            {
                case "create":
                    return new EmployeeData(CreateEmployee);
                case "read":
                    return new EmployeeData(ReadEmployee);
                case "update":
                    return new EmployeeData(UpdateEmployee);
                case "delete":
                    return new EmployeeData(DeleteEmployee);
                default:
                    return new EmployeeData(ReadEmployee);
            }
        }

        public static void CreateEmployee(Employee employee)
        {
            db.Employees.InsertOnSubmit(employee);
            db.SubmitChanges();
        }

        public static void ReadEmployee(Employee employee)
        {
            Console.WriteLine("The employee's name is: " + employee.FirstName + " " + employee.LastName);
        }

        public static void UpdateEmployee(Employee employee)
        {
            Employee employeeToUpdate = db.Employees.Where(e => e.EmployeeId == employee.EmployeeId).Single();
            employeeToUpdate.FirstName = employee.FirstName;
            employeeToUpdate.LastName = employee.LastName;
            employeeToUpdate.EmployeeNumber = employee.EmployeeNumber;
            employeeToUpdate.Email = employee.Email;         
            db.SubmitChanges();
        }

        public static void DeleteEmployee(Employee employee)
        {
            Employee employeeToDelete = db.Employees.Where(e => e.EmployeeId == employee.EmployeeId).Single();
            db.Employees.DeleteOnSubmit(employeeToDelete);
            db.SubmitChanges();
        }

        // TODO: Animal CRUD Operations
        internal static void AddAnimal(Animal animal)
        {
            db.Animals.InsertOnSubmit(animal);
            db.SubmitChanges();
        }

        internal static Animal GetAnimalByID(int id)
        {
            Animal animalById = db.Animals.Where(a => a.AnimalId >= id).First();
            return animalById;
        }

        internal static void UpdateAnimal(Animal animal, Dictionary<int, string> updates)
        {
            Animal animalToUpdate = db.Animals.Where(a => a.AnimalId == animal.AnimalId).Single();
            animalToUpdate.CategoryId = animal.CategoryId;
            animalToUpdate.Name = animal.Name;
            animalToUpdate.Age = animal.Age;
            animalToUpdate.Demeanor = animal.Demeanor;
            animalToUpdate.KidFriendly = animal.KidFriendly;
            animalToUpdate.PetFriendly = animal.PetFriendly;
            animalToUpdate.Weight = animal.Weight;
            animalToUpdate.DietPlanId = animal.DietPlanId;
            db.SubmitChanges();
        }

        internal static void RemoveAnimal(Animal animal)
        {
            Animal animalToDelete = db.Animals.Where(a => a.AnimalId == animal.AnimalId).Single();
            db.Animals.DeleteOnSubmit(animalToDelete);
            db.SubmitChanges();
        }

        // TODO: Animal Multi-Trait Search
        internal static IQueryable<Animal> SearchForAnimalByMultipleTraits(Dictionary<int, string> updates) // parameter(s)?
        {
            IQueryable<Animal> AnimalList = db.Animals;
            foreach (KeyValuePair<int, string> entry in updates)
            {
                switch (entry.Key)
                {
                    case 1:
                        var species = db.Categories.Where(s => s.Name == entry.Value).Select(p => p.CategoryId).SingleOrDefault();
                        AnimalList = AnimalList.Where(p => p.CategoryId == species);
                        break;
                    case 2:
                        AnimalList = AnimalList.Where(c => c.Name == entry.Value);
                        break;
                    case 3:
                        AnimalList = AnimalList.Where(c => c.Age.ToString() == entry.Value);
                        break;
                    case 4:
                        AnimalList = AnimalList.Where(c => c.Demeanor == entry.Value);
                        break;
                    case 5:
                        AnimalList = AnimalList.Where(c => c.KidFriendly.ToString() == entry.Value);
                        break;
                    case 6:
                        AnimalList = AnimalList.Where(c => c.PetFriendly.ToString() == entry.Value);
                        break;
                    case 7:
                        AnimalList = AnimalList.Where(c => c.Weight.ToString() == entry.Value);
                        break;
                    case 8:
                        AnimalList = AnimalList.Where(c => c.AnimalId.ToString() == entry.Value);
                        break;
                    default:
                        break;
                }    
            }
            return AnimalList;
        }

        // TODO: Misc Animal Things
        internal static int GetCategoryId(string categoryName)
        {
            int categoryId = db.Categories.Where(c => c.Name == categoryName).Select(c => c.CategoryId).FirstOrDefault();
            return categoryId;
        }

        internal static Room GetRoom(int animalId)
        {
            Room room = db.Rooms.Where(c => c.AnimalId == animalId).Single();
            return room;
        }

        internal static int GetDietPlanId(string dietPlanName)
        {

            int dietPlanId = db.DietPlans.Where(c => c.Name == dietPlanName).Select(c => c.DietPlanId).FirstOrDefault();
            return dietPlanId;

        }

        // TODO: Adoption CRUD Operations
        internal static void Adopt(Animal animal, Client client)
        {
            Adoption adoptions = new Adoption()
            {

                ClientId = client.ClientId,
                AnimalId = animal.AnimalId,
                ApprovalStatus = "Pending",
                AdoptionFee = 75,
                PaymentCollected = false
            };
            db.Adoptions.InsertOnSubmit(adoptions);
            db.SubmitChanges();
        }

        internal static IQueryable<Adoption> GetPendingAdoptions()
        {
            IQueryable<Adoption> pendingAdoptions = db.Adoptions.Where(c => c.ApprovalStatus == "Pending");
            return pendingAdoptions;
        }

        internal static void UpdateAdoption(bool isAdopted, Adoption adoption)
        {
           if ( isAdopted == true)
            {
                adoption.PaymentCollected = true;
                adoption.ApprovalStatus = "Approved";
                adoption.PaymentCollected = true;
            }
            else
            {
                adoption.PaymentCollected = false;
                adoption.ApprovalStatus = "Rejected";
                adoption.PaymentCollected = false;
            }
        }

        internal static void RemoveAdoption(int animalId, int clientId)
        {

            var deleteAdoptions = db.Adoptions.Where(c => c.AnimalId == animalId && c.ClientId == clientId).Single();

            db.Adoptions.DeleteOnSubmit(deleteAdoptions);
            try
            {
                db.SubmitChanges();
            }
            catch (NullReferenceException nullException)
            {
                Console.WriteLine(nullException); 
            }
        }

        // TODO: Shots Stuff
        internal static IQueryable<AnimalShot> GetShots(Animal animal)
        {
            IQueryable<AnimalShot> animalShots = db.AnimalShots.Where(c => c.AnimalId == animal.AnimalId);
            return animalShots;
        }

        internal static void UpdateShot(string shotName, Animal animal)
        { int shotIdentity = db.Shots.Where(c => c.Name == shotName).Select(s =>s.ShotId).Single();
            DateTime localDate = DateTime.Now;
            AnimalShot givenShots = new AnimalShot()
            {
                AnimalId = animal.AnimalId,
                ShotId = shotIdentity,
                DateReceived = localDate
            };
            db.AnimalShots.InsertOnSubmit(givenShots);
            db.SubmitChanges();
        }
        internal static void ImportCsv(string fileName)
        {
            var lines = File.ReadAllLines(fileName);
            foreach(var element in lines)
            {
                string turnTostring = element.ToString();
                string removeDoubleQuotes = turnTostring.Replace("\"", "");
                string removeSpaces = removeDoubleQuotes.Replace(" ", "");
                string[] animalTraits = removeSpaces.Split(',').ToArray(); 
                
            }
        }
        internal static void CreateAnimalFromCsv(string[] animaltraits)
        {
            Animal animal = new Animal()
            {
                Name = animaltraits[0].ToString(),
                Weight = ToNullableInt(animaltraits[1]),
                Age = ToNullableInt(animaltraits[3]),
                Demeanor = animaltraits[5],
                KidFriendly = IntToBool(int.Parse(animaltraits[7])),
                PetFriendly = IntToBool(int.Parse(animaltraits[8])),
                AdoptionStatus = AdoptionStatus(animaltraits[10])

            };
            Query.AddAnimal(animal);
        }
        internal static int? ToNullableInt(string newstring)
        {
            int number;
            if (int.TryParse(newstring, out number)) return number;
            return null;
        }
        internal static bool IntToBool(int number)
        {
            if (number == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        internal static string AdoptionStatus( string adoption)
        {
            string availability;
            if (adoption == "not adopted")
            {
                 availability = "available";
                
            }
            else
            {
                availability = "unavailable";

            }
            return availability;

        }
        // "Murdock,null,15,2,null,skittish,1,0, null, adopted,null"
        //" ""Loki""", null,18,3, null," ""cuddly""",1,1, null," ""adopted""", null
        //" ""Rowdy""", null,20,8, null," ""deceased""",1,1, null," ""not adopted""", null
    }
}
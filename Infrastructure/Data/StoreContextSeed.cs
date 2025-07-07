using System;
using System.Reflection;
using System.Text.Json;
using Core.Entities;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Data;

public class StoreContextSeed
{
    public static async Task SeedAsync(StoreContext context,UserManager<AppUser> userManager,RoleManager<IdentityRole> roleManager)
    {
       if (!await roleManager.RoleExistsAsync("Admin"))
    {
        await roleManager.CreateAsync(new IdentityRole { Name = "Admin" });
    }

    if (!await roleManager.RoleExistsAsync("Customer"))
    {
        await roleManager.CreateAsync(new IdentityRole { Name = "Customer" });
    }

    if (!userManager.Users.Any(x => x.UserName == "admin@test.com"))
    {
        var user = new AppUser
        {
            UserName = "admin@test.com",
            Email = "admin@test.com"
        };

        await userManager.CreateAsync(user, "Pa$$w0rd");
        await userManager.AddToRoleAsync(user, "Admin");
    }



        var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        if (!context.Products.Any())
        {
            var productsData = await File
            .ReadAllTextAsync(path + @"/Data/SeedData/products.json");

            var products = JsonSerializer.Deserialize<List<Product>>(productsData);

            if (products == null) return;

            context.Products.AddRange(products);

            await context.SaveChangesAsync();
        }
        
        if(!context.DeliveryMethods.Any()){
            var deliveryMethodsData=await File.ReadAllTextAsync(path + @"/Data/SeedData/delivery.json");

            var deliveryMethods=JsonSerializer.Deserialize<List<DeliveryMethod>>(deliveryMethodsData);

            if(deliveryMethods==null) return;

            context.DeliveryMethods.AddRange(deliveryMethods);

            await context.SaveChangesAsync();
        }
    }
}

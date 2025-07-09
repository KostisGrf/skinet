import { Route } from "@angular/router";
import { RegisterComponent } from "./register/register.component";
import { LoginComponent } from "./login/login.component";

export const accountRoutes:Route[]=[
    {path:'login',component:LoginComponent},
    {path:'register',component:RegisterComponent},
]
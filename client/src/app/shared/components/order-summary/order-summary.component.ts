import { Component, inject } from '@angular/core';
import { MatButton, MatIconButton } from '@angular/material/button';
import { MatFormField, MatLabel } from '@angular/material/form-field';
import { MatInput } from '@angular/material/input';
import { RouterLink } from '@angular/router';
import { CartService } from '../../../core/services/cart.service';
import { CurrencyPipe, Location, NgIf } from '@angular/common';
import { firstValueFrom } from 'rxjs';
import { StripeService } from '../../../core/services/stripe.service';
import { FormsModule } from '@angular/forms';
import { MatIcon } from '@angular/material/icon';

@Component({
  selector: 'app-order-summary',
  imports: [
    RouterLink,
    MatButton,
    MatFormField,
    MatLabel,
    MatInput,
    CurrencyPipe,
    FormsModule,
    MatIcon,
    MatIconButton,
    NgIf
  ],
  templateUrl: './order-summary.component.html',
  styleUrl: './order-summary.component.scss'
})
export class OrderSummaryComponent {
  cartService=inject(CartService);
  stripeService=inject(StripeService);
  location=inject(Location);
  code?:string;

  applyCouponCode(){
    if(!this.code) return;
    this.cartService.applyDiscount(this.code).subscribe({
      next:async coupon=>{
        const cart=this.cartService.cart();
        if(cart){
          cart.coupon=coupon;
          await firstValueFrom(this.cartService.setCart(cart));
          this.code=undefined;
        }
        if(this.location.path()==="/checkout"){
         await firstValueFrom(this.stripeService.createOrUpdatePaymentIntent());
        }
      }
    });
  }

  async removeCouponCode(){
    const cart=this.cartService.cart();
    if(!cart) return;
    if(cart.coupon) cart.coupon=undefined
    await firstValueFrom(this.cartService.setCart(cart));
    if(this.location.path()==="/checkout"){
      await firstValueFrom(this.stripeService.createOrUpdatePaymentIntent());
    }
  }

  
}

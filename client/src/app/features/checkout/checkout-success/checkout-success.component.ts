import { Component, effect, inject, OnDestroy, OnInit } from '@angular/core';
import { MatButton } from '@angular/material/button';
import { RouterLink } from '@angular/router';
import { SignalrService } from '../../../core/services/signalr.service';
import { AddressPipe } from '../../../shared/pipes/address.pipe';
import { CurrencyPipe, DatePipe, NgIf } from '@angular/common';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { PaymentPipe } from '../../../shared/pipes/payment.pipe';
import { OrderService } from '../../../core/services/order.service';

@Component({
  selector: 'app-checkout-success',
  imports: [
    MatButton,
    RouterLink,
    MatProgressSpinnerModule,
    AddressPipe,
    CurrencyPipe,
    DatePipe,
    PaymentPipe,
    NgIf

  ],
  templateUrl: './checkout-success.component.html',
  styleUrl: './checkout-success.component.scss'
})
export class CheckoutSuccessComponent implements OnDestroy ,OnInit  {
   private signalRService=inject(SignalrService);
   private orderService=inject(OrderService);
   order=this.signalRService.orderSignal();

   constructor(){
    effect(() => {
      const order = this.signalRService.orderSignal();
      if (order) {
        this.order = order;
                  }
                });}

   

    
   

   ngOnInit(): void {
       console.log("hellooooo")
   }

   ngOnDestroy(): void {
       this.orderService.orderComplete=false;
       this.signalRService.orderSignal.set(null);
   }

   
}

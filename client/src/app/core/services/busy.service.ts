import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class BusyService {
  laoding=false;

  busyRequestCount=0;

  busy(){
    this.busyRequestCount++;
    this.laoding=true;
  }

  idle(){
    this.busyRequestCount--;
    if(this.busyRequestCount<=0){
      this.busyRequestCount=0;
      this.laoding=false;
    }
    
  }
}

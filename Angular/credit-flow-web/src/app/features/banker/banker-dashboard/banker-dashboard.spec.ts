import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BankerDashboard } from './banker-dashboard';

describe('BankerDashboard', () => {
  let component: BankerDashboard;
  let fixture: ComponentFixture<BankerDashboard>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [BankerDashboard]
    })
    .compileComponents();

    fixture = TestBed.createComponent(BankerDashboard);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

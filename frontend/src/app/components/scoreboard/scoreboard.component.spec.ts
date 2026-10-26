import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ScoreboardComponent } from './scoreboard.component';

describe('ScoreboardComponent', () => {
  let fixture: ComponentFixture<ScoreboardComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ScoreboardComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(ScoreboardComponent);
    fixture.componentRef.setInput('title', 'Computer Scoreboard');
    fixture.componentRef.setInput('mode', 'Computer');
    fixture.componentRef.setInput('xLabel', 'Player (X) wins');
    fixture.componentRef.setInput('oLabel', 'Computer (O) wins');
    fixture.componentRef.setInput('drawsLabel', 'Draws');
    fixture.componentRef.setInput('xWins', 3);
    fixture.componentRef.setInput('oWins', 2);
    fixture.componentRef.setInput('draws', 1);
    fixture.detectChanges();
  });

  it('renders the supplied title, labels, and counts', () => {
    const element = fixture.nativeElement as HTMLElement;

    expect(element.querySelector('h2')?.textContent).toContain('Computer Scoreboard');
    expect(Array.from(element.querySelectorAll('dt')).map((label) => label.textContent?.trim()))
      .toEqual(['Player (X) wins', 'Computer (O) wins', 'Draws']);
    expect(Array.from(element.querySelectorAll('dd')).map((count) => count.textContent?.trim()))
      .toEqual(['3', '2', '1']);
  });

  it('emits the scoreboard mode when its reset button is clicked', () => {
    let resetMode: string | undefined;
    fixture.componentInstance.resetRequested.subscribe((mode) => {
      resetMode = mode;
    });

    (fixture.nativeElement as HTMLElement)
      .querySelector('button')
      ?.click();

    expect(resetMode).toBe('Computer');
  });
});

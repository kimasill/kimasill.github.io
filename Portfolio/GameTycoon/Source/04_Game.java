// GameTycoon/src/system/Struct/Game.java

package system.Struct;

import java.io.Serializable;

public class Game implements Serializable {
	private static final long serialVersionUID = 1L;

	private String title;
	private Subject subject;
	private Genre genre;
	private int interest;
	private int price;
	private int launchTime;
	private int cumulativeSales = 0;
	private int reviewScore;

	public Game(String title, Subject subject, Genre genre, Rule rule) {
		this.title = title;
		this.subject = subject;
		this.genre = genre;
		this.interest = rule.getInterest(subject, genre);
	}

	public String getTitle() {
		return this.title;
	}

	public Subject getSubject() {
		return subject;
	}

	public Genre getGenre() {
		return genre;
	}

	public void setLaunchTime(int time) {
		this.launchTime = time;
	}

	public void setReviewScore(int reviewScore) {
		this.reviewScore = reviewScore;
	}

	public int getReviewScore() {
		return reviewScore;
	}

	public void applyLaunchPricing(Company company, int review) {
		this.reviewScore = review;
		int baseCost = subject.getCost() + genre.getCost();
		this.price = Math.max(1,
				baseCost / 100 + interest / 12 + company.getPopularity() + review / 25);
	}

	public double getSalesAgeMultiplier(int howOld) {
		int age = Math.max(1, howOld);
		double var = Math.max(0, 10.0 / age - 1);
		double genreTail = 1.0 + (genre.ordinal() % 4) * 0.03;
		double subjectTail = 1.0 + (subject.ordinal() % 5) * 0.02;
		return (1.0 + var) * genreTail * subjectTail;
	}

	public int getInterest() {
		return this.interest;
	}

	public int getPrice() {
		return this.price;
	}

	public int getCumulativeSales() {
		return cumulativeSales;
	}

	public void setCumulativeSales(int cumulativeSales) {
		this.cumulativeSales = cumulativeSales;
	}

	public int getLaunchTime() {
		return this.launchTime;
	}
}

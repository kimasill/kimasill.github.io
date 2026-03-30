// GameTycoon/src/system/Struct/Company.java

package system.Struct;

import java.io.File;
import java.io.FileInputStream;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.ObjectInputStream;
import java.io.ObjectOutputStream;
import java.io.Serializable;
import java.text.DecimalFormat;
import java.util.ArrayList;
import java.util.Random;
import java.util.Vector;

import system.Item.Item;
import system.UI.Updateable;

public class Company implements Updateable, Serializable {
	private static final long serialVersionUID = 1L;
	private static final int MAX_PROJECT_COUNT = 3;
	private String companyName;
	private int money;
	private Vector<Game> product = new Vector(100);
	private Vector<DevGame> project = new Vector(3);
	private int projectCount;
	private int popularity;
	private Rule rule;
	private int time = 0;
	private int devSpeed = 0;
	private int bill = 100;
	private int floor = 1;
	private int itemTabTime = -13;
	private int devTabTime = -13;

	ArrayList<Item> itemSellList = new ArrayList<Item>();
	ArrayList<Developer> devSellList = new ArrayList<Developer>();
	ArrayList<Item> itemList = new ArrayList<Item>();
	ArrayList<Developer> devList = new ArrayList<Developer>();

	public Company(String companyName, Developer defaultDev, int time, Rule rule) {
		this.companyName = companyName;
		this.projectCount = 0;
		this.devList.add(defaultDev);
		this.money = 200;
		this.popularity = 1;
		this.rule = rule != null ? rule : new Rule();
		this.time += time;
		gameUpdater.signal(this);
		setItemSellList();
		setDevSellList();
	}

	public int getFloor() {
		return this.floor;
	}

	public boolean addFloor() {
		if (this.floor >= 11) {
			return false;
		}
		this.floor++;
		return true;
	}

	public boolean saveFile(String fileName) {
		try {
			new File("save").mkdirs();
			FileOutputStream fo = new FileOutputStream("save/" + fileName + ".save");
			ObjectOutputStream save = new ObjectOutputStream(fo);
			save.writeObject(this);
			save.close();
			fo.close();
		} catch (IOException e) {
			return false;
		}
		return true;
	}

	public boolean loadFile(String fileName) throws ClassNotFoundException {
		FileInputStream fo = null;
		Company com;
		try {
			fo = new FileInputStream("save/" + fileName);
			ObjectInputStream load = new ObjectInputStream(fo);
			com = (Company) load.readObject();
			companyName = com.companyName;
			money = com.money;
			product = com.product;
			project = com.project;
			projectCount = com.projectCount;
			popularity = com.popularity;
			itemList = com.itemList;
			devList = com.devList;
			rule = com.rule;
			if (rule == null)
				rule = new Rule();
			time = com.time;
			devSpeed = com.devSpeed;
			floor = com.floor;
			itemTabTime = com.itemTabTime;
			devTabTime = com.devTabTime;
			itemSellList = com.itemSellList != null ? com.itemSellList : new ArrayList<Item>();
			devSellList = com.devSellList != null ? com.devSellList : new ArrayList<Developer>();
			bill = com.bill;
			fo.close();
			load.close();
			gameUpdater.signal(this);

		} catch (IOException e) {
			return false;
		}

		return true;
	}

	public ArrayList<Developer> getDevSellList() {
		return this.devSellList;
	}

	public ArrayList<Item> getItemSellList() {
		return this.itemSellList;
	}

	public void setItemTabTime(int iTime) {
		this.itemTabTime = iTime;
	}

	public int getItemTabTime() {
		return this.itemTabTime;
	}

	public void setDevTabTime(int dTime) {
		this.devTabTime = dTime;
	}

	public int getDevTabTime() {
		return this.devTabTime;
	}

	public String getCompanyName() {
		return companyName;
	}

	public int getMoney() {
		return money;
	}

	public String getMoneyToString() {
		DecimalFormat formatter = new DecimalFormat("###,###");
		return formatter.format(this.money);
	}

	public int getTime() {
		return this.time;
	}

	public void setTime(int time) {
		this.time += time;
		gameUpdater.signal(this);
	}

	public void setMoney(int money) {
		this.money = money;
		gameUpdater.signal(this);
	}

	public boolean appendMoney(int money) {
		this.setMoney(this.money + money);
		if (this.money < 0) {
			return false;
		}
		return true;
	}

	public DevGame getProject(int i) {
		try {
			return project.get(i);
		} catch (ArrayIndexOutOfBoundsException e) {
			return null;
		}

	}

	public DevGame[] getProjects() {
		return project.toArray(new DevGame[0]);
	}

	public int getProjectCount() {
		return this.projectCount;
	}

	public Developer[] devListToArray() {
		return devList.toArray(new Developer[0]);
	}

	public Developer getDev(int n) {
		return devList.get(n);
	}

	public int startDev(String gameTitle, Subject subject, Genre genre, Developer[] team) {
		int cost;
		try {
			cost = subject.getCost() + genre.getCost();
		} catch (NullPointerException e) {
			return 5;
		}

		if (this.money - cost < 0)
			return 1;
		for (Game game : this.product)
			if (game.getTitle().equals(gameTitle))
				return 3;
		for (DevGame game : this.project) {
			if (game.getTitle().equals(gameTitle))
				return 3;
		}
		if (gameTitle.equals(""))
			return 3;
		if (team[0] == null)
			return 2;
		if (projectCount > 2)
			return 4;

		this.appendMoney(-cost);
		Game newGame = new Game(gameTitle, subject, genre, rule);
		project.add(new DevGame(newGame, team));
		projectCount++;
		for (Developer dev : team) {
			if (dev == null)
				break;
			dev.setWorkable(false);
		}
		gameUpdater.signal(this);
		return 0;

	}

	public void progressProject() {
		for (DevGame dg : project) {
			if (dg != null)
				dg.addProgress();
		}
		gameUpdater.signal(this);
	}

	public int computeLaunchReview(DevGame dg) {
		Game g = dg.getGame();
		int teamSum = 0;
		int n = 0;
		for (Developer d : dg.getTeam()) {
			if (d == null)
				break;
			teamSum += d.getAbility();
			n++;
		}
		int avgAbility = n > 0 ? teamSum / n : 0;
		Random r = new Random();
		int base = g.getInterest() / 4 + avgAbility / 4;
		return Math.max(1, Math.min(100, base + r.nextInt(15) - 7));
	}

	private void adjustPopularityFromReview(int review) {
		int delta = (review - 50) / 15;
		popularity = Math.max(1, Math.min(200, popularity + delta));
	}

	public int getPopularity() {
		return popularity;
	}

	public void launchGame(DevGame game, int reviewScore) {
		Game newGame = game.getGame();
		newGame.setLaunchTime(this.time);
		newGame.applyLaunchPricing(this, reviewScore);
		adjustPopularityFromReview(reviewScore);
		product.add(newGame);
		project.remove(game);
		projectCount--;
		for (Developer dev : game.getTeam()) {
			if (dev == null)
				break;
			dev.setWorkable(true);
		}
		gameUpdater.signal(this);
	}

	public void sellGame() {
		Random rand = new Random();
		for (Game g : product) {
			int howOld = this.time - g.getLaunchTime();
			if (howOld > 90)
				continue;
			int ageDays = Math.max(1, howOld);
			double var = 10.0 / ageDays - 1;
			if (var < 0)
				var = 0;
			int salesVolume = g.getInterest() * this.popularity;
			double dailyNoise = 1.0 + rand.nextDouble() * 0.15;
			salesVolume = (int) (salesVolume * (1.0 + var) * dailyNoise * g.getSalesAgeMultiplier(howOld));
			if (salesVolume < 0)
				salesVolume = 0;
			this.appendMoney(g.getPrice() * salesVolume);
			g.setCumulativeSales(g.getCumulativeSales() + salesVolume);
		}
	}

	public ArrayList<Item> getItemList() {
		return this.itemList;
	}

	public void addItemList(Item nonePlacedItem) {
		this.itemList.add(nonePlacedItem);
	}

	public void setItemSellList() {
		this.itemSellList.clear();
		for (int i = 0; i < 3; i++) {
			this.itemSellList.add(new Item());
		}
	}

	public void setDevSellList() {
		this.devSellList.clear();
		for (int i = 0; i < 4; i++) {
			this.devSellList.add(new Developer());
		}
	}

	public void setStatus3(int efficiency) {
		this.devSpeed += efficiency;
	}

	public void addDevList(Developer dev) {
		devList.add(dev);
	}

	public ArrayList<Developer> getDevList() {
		return this.devList;
	}

	public Vector<Developer> getFreeDevList() {
		Vector<Developer> freeDevs = new Vector<Developer>();
		for (int i = 0; i < devList.size(); i++) {
			if (devList.get(i).isWorkable()) {
				freeDevs.add(devList.get(i));
			}
		}
		return freeDevs;

	}

	public boolean adjustment() {
		int costOfMaintance = 0;
		int totalSalary = 0;
		for (Item item : this.itemList) {
			costOfMaintance += item.getStatus2();
		}
		for (Developer dev : this.devList) {
			totalSalary += dev.getSalary();
		}
		return this.appendMoney(-(costOfMaintance + totalSalary + bill));

	}

	public Vector<Game> getProducts() {
		return product;
	}

}

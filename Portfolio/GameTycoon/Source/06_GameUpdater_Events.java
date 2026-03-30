// GameTycoon/src/system/UI/GameUpdater.java

package system.UI;

import java.awt.Component;

import javax.swing.JLabel;
import javax.swing.JLayeredPane;
import javax.swing.JProgressBar;

import system.Struct.Company;
import system.Tab.DevEventTab;

public class GameUpdater extends Thread implements GameUI {
	private Object object;
	private JLabel label;

	public GameUpdater() {
		this.start();
	}

	@Override
	public void run() {
		while (true) {
			try {
				synchronized (this) {
					this.wait();
					statusBarUpdate();
					progressBarUpdate();
				}
			} catch (InterruptedException e) {
				e.printStackTrace();
			}
		}
	}

	public void signal(Object object) {
		this.object = object;
		synchronized (this) {
			this.notify();
		}
	}

	public void signal(Object object, JLabel label) {
		this.object = object;
		this.label = label;
		synchronized (this) {
			this.notify();
		}
	}

	private void statusBarUpdate() {
		Company com = (Company) object;
		statusPane_money.setText("" + com.getMoneyToString());
		statusPane_date.setText("" + com.getTime());

	}

	private void progressBarUpdate() {
		Company com = (Company) object;

		Component c[] = progressPane.getComponents();

		for (int i = 0; i < 6; i += 2) {
			JLabel label = (JLabel) c[i];
			JProgressBar progress = (JProgressBar) c[i + 1];
			if (com.getProject(i / 2) == null) {
				label.setText("");
				progress.setValue(0);
				label.setVisible(false);
				progress.setVisible(false);
			} else {
				label.setText(com.getProject(i / 2).getTitle());
				progress.setValue(com.getProject(i / 2).getProgress());
				label.setVisible(true);
				progress.setVisible(true);
				if (progress.getValue() != 0 && progress.getValue() % 20 == 0 && progress.getValue() < 100)
					if (Math.random() < 0.5)
						layeredPane.add(new DevEventTab(com, i / 2), JLayeredPane.POPUP_LAYER);
			}
		}
	}

	private void mapUpdate() {

	}
}

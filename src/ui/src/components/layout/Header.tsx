import { NavLink } from 'react-router-dom';
import styles from './Header.module.css';

export function Header() {
  return (
    <header className={styles.header}>
      <div className={styles.inner}>
        <div className={styles.brand}>
          <span className={styles.logo}>✦</span>
          <span className={styles.title}>OCAS Tracker</span>
        </div>
        <nav className={styles.nav}>
          <NavLink
            to="/tasks"
            className={({ isActive }) =>
              [styles.link, isActive ? styles.active : ''].join(' ')
            }
          >
            Tasks
          </NavLink>
          <NavLink
            to="/summary"
            className={({ isActive }) =>
              [styles.link, isActive ? styles.active : ''].join(' ')
            }
          >
            Today's Summary
          </NavLink>
        </nav>
      </div>
    </header>
  );
}

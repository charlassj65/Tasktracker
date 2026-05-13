import type { TaskSummary } from '../../types/task';
import styles from './TodaySummary.module.css';

interface Props {
  summary: TaskSummary;
}

export function TodaySummary({ summary }: Props) {
  const date = new Date(summary.date).toLocaleDateString(undefined, {
    weekday: 'long',
    year: 'numeric',
    month: 'long',
    day: 'numeric',
  });

  return (
    <div className={styles.card}>
      <div className={styles.icon}>✦</div>
      <p className={styles.date}>{date}</p>
      <p className={styles.count}>
        {summary.totalTasks === 0
          ? 'No tasks due today'
          : `${summary.totalTasks} task${summary.totalTasks > 1 ? 's' : ''} due today`}
      </p>
      <p className={styles.summary}>{summary.summary}</p>
    </div>
  );
}

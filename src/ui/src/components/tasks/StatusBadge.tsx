import type { TaskStatus } from '../../types/task';
import styles from './StatusBadge.module.css';

const LABELS: Record<TaskStatus, string> = {
  Todo: 'Todo',
  InProgress: 'In Progress',
  Done: 'Done',
};

interface Props {
  status: TaskStatus;
}

export function StatusBadge({ status }: Props) {
  return (
    <span className={[styles.badge, styles[status]].join(' ')}>
      {LABELS[status]}
    </span>
  );
}

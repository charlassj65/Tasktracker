import type { Task, CreateTaskRequest } from '../../types/task';
import { TaskForm } from './TaskForm';
import styles from './TaskModal.module.css';

interface Props {
  title: string;
  task?: Task;
  onSubmit: (data: CreateTaskRequest) => Promise<void>;
  onClose: () => void;
  submitting: boolean;
}

export function TaskModal({ title, task, onSubmit, onClose, submitting }: Props) {
  return (
    <div className={styles.overlay} onClick={e => e.target === e.currentTarget && onClose()}>
      <div className={styles.modal}>
        <div className={styles.header}>
          <h2 className={styles.title}>{title}</h2>
          <button className={styles.close} onClick={onClose} aria-label="Close">✕</button>
        </div>
        <div className={styles.body}>
          <TaskForm
            initial={task}
            onSubmit={onSubmit}
            onCancel={onClose}
            submitting={submitting}
          />
        </div>
      </div>
    </div>
  );
}

import type { Task } from '../../types/task';
import { StatusBadge } from './StatusBadge';
import styles from './TaskList.module.css';

interface Props {
  tasks: Task[];
  onEdit: (task: Task) => void;
  onDelete: (task: Task) => void;
}

function formatDate(iso?: string) {
  if (!iso) return '—';
  return new Date(iso).toLocaleDateString(undefined, {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
  });
}

export function TaskList({ tasks, onEdit, onDelete }: Props) {
  if (tasks.length === 0) {
    return (
      <div className={styles.empty}>
        <p>No tasks yet. Create one to get started.</p>
      </div>
    );
  }

  return (
    <div className={styles.tableWrapper}>
      <table className={styles.table}>
        <thead>
          <tr>
            <th>Title</th>
            <th>Description</th>
            <th>Status</th>
            <th>Due Date</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          {tasks.map(task => (
            <tr key={task.id}>
              <td className={styles.titleCell}>{task.title}</td>
              <td className={styles.descCell}>
                {task.description ?? <span className={styles.none}>—</span>}
              </td>
              <td>
                <StatusBadge status={task.status} />
              </td>
              <td className={styles.dateCell}>{formatDate(task.dueDate)}</td>
              <td>
                <div className={styles.btnGroup}>
                  <button
                    className={styles.btnEdit}
                    onClick={() => onEdit(task)}
                    aria-label="Edit task"
                  >
                    Edit
                  </button>
                  <button
                    className={styles.btnDelete}
                    onClick={() => onDelete(task)}
                    aria-label="Delete task"
                  >
                    Delete
                  </button>
                </div>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

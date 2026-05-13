import { useEffect, useState, useCallback } from 'react';
import { taskApi } from '../api/taskApi';
import type { Task, CreateTaskRequest, UpdateTaskRequest } from '../types/task';
import { TaskList } from '../components/tasks/TaskList';
import { TaskModal } from '../components/tasks/TaskModal';
import styles from './TasksPage.module.css';

type ModalState =
  | { kind: 'closed' }
  | { kind: 'create' }
  | { kind: 'edit'; task: Task };

export function TasksPage() {
  const [tasks, setTasks] = useState<Task[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [modal, setModal] = useState<ModalState>({ kind: 'closed' });
  const [submitting, setSubmitting] = useState(false);
  const [deleteConfirm, setDeleteConfirm] = useState<Task | null>(null);

  const loadTasks = useCallback(async () => {
    try {
      setError('');
      const data = await taskApi.getAll();
      setTasks(data);
    } catch {
      setError('Failed to load tasks. Is the API running?');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { loadTasks(); }, [loadTasks]);

  const handleCreate = async (data: CreateTaskRequest) => {
    setSubmitting(true);
    try {
      await taskApi.create(data);
      setModal({ kind: 'closed' });
      await loadTasks();
    } finally {
      setSubmitting(false);
    }
  };

  const handleUpdate = async (data: UpdateTaskRequest) => {
    if (modal.kind !== 'edit') return;
    setSubmitting(true);
    try {
      await taskApi.update(modal.task.id, data);
      setModal({ kind: 'closed' });
      await loadTasks();
    } finally {
      setSubmitting(false);
    }
  };

  const handleDelete = async () => {
    if (!deleteConfirm) return;
    try {
      await taskApi.delete(deleteConfirm.id);
      setDeleteConfirm(null);
      await loadTasks();
    } catch {
      setError('Failed to delete task.');
    }
  };

  return (
    <div>
      <div className={styles.pageHeader}>
        <div>
          <h1 className={styles.heading}>Tasks</h1>
          <p className={styles.sub}>{tasks.length} task{tasks.length !== 1 ? 's' : ''} total</p>
        </div>
        <button
          className={styles.btnCreate}
          onClick={() => setModal({ kind: 'create' })}
        >
          + New Task
        </button>
      </div>

      {error && <p className={styles.error}>{error}</p>}

      {loading ? (
        <p className={styles.loading}>Loading tasks…</p>
      ) : (
        <TaskList
          tasks={tasks}
          onEdit={task => setModal({ kind: 'edit', task })}
          onDelete={task => setDeleteConfirm(task)}
        />
      )}

      {modal.kind === 'create' && (
        <TaskModal
          title="Create Task"
          onSubmit={handleCreate}
          onClose={() => setModal({ kind: 'closed' })}
          submitting={submitting}
        />
      )}

      {modal.kind === 'edit' && (
        <TaskModal
          title="Edit Task"
          task={modal.task}
          onSubmit={handleUpdate}
          onClose={() => setModal({ kind: 'closed' })}
          submitting={submitting}
        />
      )}

      {deleteConfirm && (
        <div className={styles.overlay} onClick={e => e.target === e.currentTarget && setDeleteConfirm(null)}>
          <div className={styles.confirmBox}>
            <h3 className={styles.confirmTitle}>Delete Task</h3>
            <p className={styles.confirmMsg}>
              Are you sure you want to delete{' '}
              <strong>"{deleteConfirm.title}"</strong>?
              This cannot be undone.
            </p>
            <div className={styles.confirmActions}>
              <button
                className={styles.btnSecondary}
                onClick={() => setDeleteConfirm(null)}
              >
                Cancel
              </button>
              <button className={styles.btnDanger} onClick={handleDelete}>
                Delete
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

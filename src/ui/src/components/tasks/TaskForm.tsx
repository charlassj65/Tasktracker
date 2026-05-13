import { useState, useEffect } from 'react';
import type { Task, CreateTaskRequest, TaskStatus } from '../../types/task';
import styles from './TaskForm.module.css';

interface Props {
  initial?: Task;
  onSubmit: (data: CreateTaskRequest) => Promise<void>;
  onCancel: () => void;
  submitting: boolean;
}

const STATUS_OPTIONS: TaskStatus[] = ['Todo', 'InProgress', 'Done'];
const STATUS_LABELS: Record<TaskStatus, string> = {
  Todo: 'Todo',
  InProgress: 'In Progress',
  Done: 'Done',
};

export function TaskForm({ initial, onSubmit, onCancel, submitting }: Props) {
  const [title, setTitle] = useState(initial?.title ?? '');
  const [description, setDescription] = useState(initial?.description ?? '');
  const [status, setStatus] = useState<TaskStatus>(initial?.status ?? 'Todo');
  const [dueDate, setDueDate] = useState(
    initial?.dueDate ? initial.dueDate.slice(0, 10) : ''
  );
  const [error, setError] = useState('');

  useEffect(() => {
    if (initial) {
      setTitle(initial.title);
      setDescription(initial.description ?? '');
      setStatus(initial.status);
      setDueDate(initial.dueDate ? initial.dueDate.slice(0, 10) : '');
    }
  }, [initial]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    if (!title.trim()) {
      setError('Title is required.');
      return;
    }
    if (title.trim().length > 100) {
      setError('Title must be 100 characters or fewer.');
      return;
    }

    try {
      await onSubmit({
        title: title.trim(),
        description: description.trim() || undefined,
        status,
        dueDate: dueDate ? new Date(dueDate).toISOString() : undefined,
      });
    } catch (err: unknown) {
      const msg =
        (err as { response?: { data?: { detail?: string } } })?.response?.data?.detail ??
        'Something went wrong.';
      setError(msg);
    }
  };

  return (
    <form className={styles.form} onSubmit={handleSubmit} noValidate>
      <div className={styles.field}>
        <label className={styles.label}>Title *</label>
        <input
          className={styles.input}
          value={title}
          onChange={e => setTitle(e.target.value)}
          maxLength={100}
          placeholder="What needs to be done?"
          autoFocus
        />
        <span className={styles.counter}>{title.length}/100</span>
      </div>

      <div className={styles.field}>
        <label className={styles.label}>Description</label>
        <textarea
          className={styles.textarea}
          value={description}
          onChange={e => setDescription(e.target.value)}
          placeholder="Optional details…"
          rows={3}
        />
      </div>

      <div className={styles.row}>
        <div className={styles.field}>
          <label className={styles.label}>Status</label>
          <select
            className={styles.select}
            value={status}
            onChange={e => setStatus(e.target.value as TaskStatus)}
          >
            {STATUS_OPTIONS.map(s => (
              <option key={s} value={s}>
                {STATUS_LABELS[s]}
              </option>
            ))}
          </select>
        </div>

        <div className={styles.field}>
          <label className={styles.label}>Due Date</label>
          <input
            type="date"
            className={styles.input}
            value={dueDate}
            onChange={e => setDueDate(e.target.value)}
          />
        </div>
      </div>

      {error && <p className={styles.error}>{error}</p>}

      <div className={styles.actions}>
        <button
          type="button"
          className={styles.btnSecondary}
          onClick={onCancel}
          disabled={submitting}
        >
          Cancel
        </button>
        <button type="submit" className={styles.btnPrimary} disabled={submitting}>
          {submitting ? 'Saving…' : initial ? 'Save Changes' : 'Create Task'}
        </button>
      </div>
    </form>
  );
}

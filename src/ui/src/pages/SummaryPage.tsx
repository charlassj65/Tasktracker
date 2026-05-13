import { useEffect, useState } from 'react';
import { taskApi } from '../api/taskApi';
import type { TaskSummary } from '../types/task';
import { TodaySummary } from '../components/summary/TodaySummary';
import styles from './SummaryPage.module.css';

export function SummaryPage() {
  const [summary, setSummary] = useState<TaskSummary | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const load = async () => {
    setLoading(true);
    setError('');
    try {
      const data = await taskApi.getTodaySummary();
      setSummary(data);
    } catch {
      setError('Failed to load summary. Is the API running?');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { load(); }, []);

  return (
    <div>
      <div className={styles.pageHeader}>
        <h1 className={styles.heading}>Today's Summary</h1>
        <button className={styles.btnRefresh} onClick={load} disabled={loading}>
          {loading ? 'Loading…' : '↻ Refresh'}
        </button>
      </div>

      {error && <p className={styles.error}>{error}</p>}

      {!loading && summary && <TodaySummary summary={summary} />}

      {!loading && !summary && !error && (
        <p className={styles.empty}>No summary available.</p>
      )}
    </div>
  );
}

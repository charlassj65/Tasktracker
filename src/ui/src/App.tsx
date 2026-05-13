import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { Layout } from './components/layout/Layout';
import { TasksPage } from './pages/TasksPage';
import { SummaryPage } from './pages/SummaryPage';

export default function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route element={<Layout />}>
          <Route index element={<Navigate to="/tasks" replace />} />
          <Route path="tasks" element={<TasksPage />} />
          <Route path="summary" element={<SummaryPage />} />
        </Route>
      </Routes>
    </BrowserRouter>
  );
}

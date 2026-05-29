-- Task Manager — schema and seed data (Phase 0)
-- Demo credentials: demo@taskmanager.local / Demo123!

CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- Bootstrap system user for seed audit columns
-- Demo user id used after users table is populated

CREATE TABLE roles (
    id          SERIAL PRIMARY KEY,
    name        VARCHAR(100) NOT NULL UNIQUE,
    created_by  UUID NOT NULL,
    created_on  TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_by  UUID NOT NULL,
    updated_on  TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE task_statuses (
    id          SERIAL PRIMARY KEY,
    name        VARCHAR(50) NOT NULL UNIQUE,
    description VARCHAR(200),
    sort_order  INT NOT NULL DEFAULT 0,
    is_active   BOOLEAN NOT NULL DEFAULT TRUE,
    created_by  UUID NOT NULL,
    created_on  TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_by  UUID NOT NULL,
    updated_on  TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE users (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    email           VARCHAR(256) NOT NULL UNIQUE,
    name            VARCHAR(200) NOT NULL,
    alias           VARCHAR(50) UNIQUE,
    password_hash   VARCHAR(256),
    role_id         INT NOT NULL REFERENCES roles(id),
    is_active       BOOLEAN NOT NULL DEFAULT TRUE,
    end_date        TIMESTAMPTZ,
    last_login_date TIMESTAMPTZ,
    mfa_enabled     BOOLEAN NOT NULL DEFAULT FALSE,
    created_by      UUID NOT NULL,
    created_on      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_by      UUID NOT NULL,
    updated_on      TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE tasks (
    id          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id     UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    title       VARCHAR(200) NOT NULL,
    description VARCHAR(2000),
    status_id   INT NOT NULL REFERENCES task_statuses(id),
    priority    VARCHAR(20) NOT NULL CHECK (priority IN ('High', 'Standard', 'Low')),
    due_date    TIMESTAMPTZ NOT NULL,
    created_by  UUID NOT NULL,
    created_on  TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_by  UUID NOT NULL,
    updated_on  TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX ix_tasks_user_id ON tasks(user_id);
CREATE INDEX ix_tasks_status_id ON tasks(status_id);
CREATE INDEX ix_users_role_id ON users(role_id);

-- Seed constants
-- System bootstrap id for lookup tables
-- Demo user: 11111111-1111-1111-1111-111111111111

INSERT INTO roles (id, name, created_by, created_on, updated_by, updated_on) VALUES
    (1, 'ProjectManager', '00000000-0000-0000-0000-000000000000', NOW(), '00000000-0000-0000-0000-000000000000', NOW()),
    (2, 'Admin',          '00000000-0000-0000-0000-000000000000', NOW(), '00000000-0000-0000-0000-000000000000', NOW());

SELECT setval('roles_id_seq', (SELECT MAX(id) FROM roles));

INSERT INTO task_statuses (id, name, description, sort_order, is_active, created_by, created_on, updated_by, updated_on) VALUES
    (1, 'Todo',        'Not started',        1, TRUE, '00000000-0000-0000-0000-000000000000', NOW(), '00000000-0000-0000-0000-000000000000', NOW()),
    (2, 'InProgress',  'Work in progress',   2, TRUE, '00000000-0000-0000-0000-000000000000', NOW(), '00000000-0000-0000-0000-000000000000', NOW()),
    (3, 'Done',        'Completed',          3, TRUE, '00000000-0000-0000-0000-000000000000', NOW(), '00000000-0000-0000-0000-000000000000', NOW());

SELECT setval('task_statuses_id_seq', (SELECT MAX(id) FROM task_statuses));

INSERT INTO users (
    id, email, name, alias, password_hash, role_id, is_active, end_date, last_login_date, mfa_enabled,
    created_by, created_on, updated_by, updated_on
) VALUES (
    '11111111-1111-1111-1111-111111111111',
    'demo@taskmanager.local',
    'Demo PM',
    'demo-pm',
    '$2a$11$EkrRNmGcFywkUs9u4DXvgOZ351v0fdJjRz0FHkF5vnFkSnWZdy8nG',
    1,
    TRUE,
    NULL,
    NULL,
    FALSE,
    '11111111-1111-1111-1111-111111111111',
    NOW(),
    '11111111-1111-1111-1111-111111111111',
    NOW()
);

INSERT INTO tasks (id, user_id, title, description, status_id, priority, due_date, created_by, created_on, updated_by, updated_on) VALUES
    (
        '22222222-2222-2222-2222-222222222221',
        '11111111-1111-1111-1111-111111111111',
        'Urgent client deliverable',
        'High-priority item due within 48 hours.',
        1,
        'High',
        NOW() + INTERVAL '24 hours',
        '11111111-1111-1111-1111-111111111111',
        NOW(),
        '11111111-1111-1111-1111-111111111111',
        NOW()
    ),
    (
        '22222222-2222-2222-2222-222222222222',
        '11111111-1111-1111-1111-111111111111',
        'Sprint planning prep',
        'Standard priority work due next month.',
        2,
        'Standard',
        NOW() + INTERVAL '30 days',
        '11111111-1111-1111-1111-111111111111',
        NOW(),
        '11111111-1111-1111-1111-111111111111',
        NOW()
    ),
    (
        '22222222-2222-2222-2222-222222222223',
        '11111111-1111-1111-1111-111111111111',
        'Quarterly documentation review',
        'Low priority backlog item due in six months.',
        3,
        'Low',
        NOW() + INTERVAL '180 days',
        '11111111-1111-1111-1111-111111111111',
        NOW(),
        '11111111-1111-1111-1111-111111111111',
        NOW()
    );

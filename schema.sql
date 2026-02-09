-- departments
CREATE TABLE department (
                            id   INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
                            name VARCHAR(200) NOT NULL
);

-- employees
CREATE TABLE employee (
                          id            INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
                          name          VARCHAR(200) NOT NULL,
                          department_id INTEGER REFERENCES department(id),
                          username      VARCHAR(100) UNIQUE NOT NULL,
                          password      TEXT    -- сейчас хранит plain text
);

-- chat rooms
CREATE TABLE chatroom (
                          id    INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
                          topic VARCHAR(255)
);

-- связи "комната — участник" (many-to-many)
CREATE TABLE chatroom_members (
                                  chatroom_id INTEGER REFERENCES chatroom(id) ON DELETE CASCADE,
                                  employee_id INTEGER REFERENCES employee(id)   ON DELETE CASCADE,
                                  joined_at   TIMESTAMPTZ DEFAULT now(),
                                  PRIMARY KEY (chatroom_id, employee_id)
);

-- сообщения
CREATE TABLE chat_message (
                              id         INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
                              sender_id  INTEGER REFERENCES employee(id),
                              chatroom_id INTEGER REFERENCES chatroom(id) ON DELETE CASCADE,
                              created_at TIMESTAMPTZ DEFAULT now(),
                              message    TEXT NOT NULL
);

-- (если ты добавлял триггер для min 2 members — он тоже в схеме)
